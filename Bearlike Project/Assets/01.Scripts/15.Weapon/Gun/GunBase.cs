using System;
using System.Collections.Generic;
using Data;
using Status;
using Fusion;
using Manager;
using Player;
using UnityEngine;
using UnityEngine.VFX;
using Weapon.Bullet;

namespace Weapon.Gun
{
    public class GunBase : WeaponBase, IJsonData<GunJsonData>
    {
        #region Static

        // Info Data 캐싱
        private static readonly Dictionary<int, GunJsonData> InfoDataCash = new Dictionary<int, GunJsonData>();
        public static void AddInfoData(int id, GunJsonData data) => InfoDataCash.TryAdd(id, data);
        public static GunJsonData GetInfoData(int id) => InfoDataCash.TryGetValue(id, out var data) ? data : new GunJsonData();
        public static void ClearInfosData() => InfoDataCash.Clear();
        
        // Status Data 캐싱
        private static readonly Dictionary<int, StatusJsonData> StatusDataChasing = new Dictionary<int, StatusJsonData>();
        public static void AddStatusData(int id, StatusJsonData data) => StatusDataChasing.TryAdd(id, data);
        public static StatusJsonData GetStatusData(int id) => StatusDataChasing.TryGetValue(id, out var data) ? data : new StatusJsonData();
        public static void ClearStatusData() => StatusDataChasing.Clear();

        #endregion
        
        private Camera _camera;

        [Header("총 정보")] 
        public int id;
        public string explain;
        
        [Header("총 이펙트")] 
        public VisualEffect shootEffect; // 발사 이펙트
        
        [Header("사운드")]
        public AudioSource shootSound;
        public AudioSource emptyAmmoSound;
        public AudioSource reloadSound;

        [Header("총알")] 
        public BulletBase bullet;
        public static StatusValue<int> ammo = new StatusValue<int>(){Max = 100, Current = int.MaxValue};
        public StatusValue<int> magazine = new StatusValue<int>() {Max = 10, Current = 10}; // max 최대 탄약, current 현재 장정된 탄약

        public float bulletFirePerMinute; // 분당 총알 발사량
        [Networked] public TickTimer FireLateTimer { get; set; }
        [Networked] public TickTimer ReloadLateTimer { get; set; }
        public float fireLateSecond;
        public float reloadLateSecond;
        public Transform fireTransform;

        // 총을 쏘면 Bullet을 스폰하는데 스폰하기 전에 bullet에 적용할 메서드
        // 해당 메서드는 총의 장착을 해제하면 null로 함
        public Action<BulletBase> BeforeShootAction;

        /// <summary>
        /// 총을 쏘고 난 뒤에 동작하게 할 Action
        /// 정상적으로 총이 발사 되었을때만 동작함
        /// </summary>
        public Action AfterShootAction;

        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();
            
            // Json Data 가져오기
            SetJsonData(GetInfoData(id));
            var statusData = GetStatusData(id);
            status.SetJsonData(statusData);
            bulletFirePerMinute = statusData.GetInt("Bullet Fire Per Minute");  
            reloadLateSecond = statusData.GetFloat("Reload Late Second");
            magazine.Max = statusData.GetInt("Magazine Max");
            magazine.Current = magazine.Max;

            EquipAction += SetCamera;
            ReleaseEquipAction += (obj) =>
            {
                AfterShootAction = null;  
                BeforeShootAction = null;
                status.ClearAdditionalStatus();
            };
        }
        
        public override void Start()
        {
            base.Start();
            IsGun = true;
            
            BulletInit();
        }

        public override void Spawned()
        {
            base.Spawned();
            AttackAction += ShootRPC;
            FireLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
            ReloadLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }

        #endregion

        private void SetCamera(GameObject equipObject)
        {
            if (equipObject.TryGetComponent(out PlayerCameraController pcc))
            {
                _camera = pcc.targetCamera;
            }
        }

        public virtual void Shoot()
        {
            if (FireLateTimer.Expired(Runner))
            {
                FireLateTimer = TickTimer.CreateFromSeconds(Runner, fireLateSecond);
                if (magazine.Current != 0)
                {
                    var dst = CheckRay();
                    
                    if(shootEffect != null) shootEffect.Play();
                    // bullet.destination = dst;
                    // bullet.hitEffect = hitEffect;
                    // bullet.bknock = false;
                    // bullet.maxMoveDistance = status.attackRange;
                    // BeforeShootAction?.Invoke(bullet);

                    if (HasStateAuthority)
                    {
                        Runner.SpawnAsync(bullet.gameObject, fireTransform.position, fireTransform.rotation, null,
                            (runner, o) =>
                            {
                                var b = o.GetComponent<BulletBase>();

                                b.OwnerId = OwnerId;
                                b.destination = dst;
                                b.hitEffect = hitEffect;
                                b.bknock = false;
                                
                                BeforeShootAction?.Invoke(b);
                            });
                    }

                    SetMagazineRPC(StatusValueType.Current, --magazine.Current);
                    SoundManager.Play(shootSound);
                    
                    AfterShootAction?.Invoke();
                }
                else
                {
                    SoundManager.Play(emptyAmmoSound);
                }
            }
        }

        // 카메라가 바라보는 방향으로 직선 레이를 쏜다.    
        public Vector3 CheckRay()
        {
            Vector3 detination = Vector3.zero;
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            DebugManager.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
            
            return ray.direction;
            
            // var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            // if(Runner.LagCompensation.Raycast(ray.origin, ray.direction, float.MaxValue, Object.InputAuthority, out var hit, includeCollide, hitOptions))
            // {
            //     DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.GameObject.name}");
            //
            //     StatusBase hitState;
            //     if (hit.GameObject.TryGetComponent(out hitState))
            //     {
            //         // hitState.ApplyDamage(state.attack.Current, (ObjectProperty)state.property); // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
            //         // hitState.ApplyDamageRPC(status.attack.Current, (CrowdControl)status.property);
            //         var hitEffectObject = Instantiate(hitEffect.gameObject, hit.Point, Quaternion.identity);
            //         hitEffectObject.transform.LookAt(hit.Normal);
            //         Destroy(hitEffectObject, 5f);
            //     }
            //     
            //     detination = hit.Point;
            // }
            // else
            // {
            //     detination = ray.direction * int.MaxValue;
            // }
            //
            // return detination;
        }

        #region Bullet Funtion

        public virtual void BulletInit()
        {
            magazine.Current = int.MaxValue;

            fireLateSecond = 60 / bulletFirePerMinute;
        }

        public virtual void ReLoadBullet(int bulletAmount = int.MaxValue)
        {
            if (ReloadLateTimer.Expired(Runner) && ammo.isMin == false)
            {
                ReloadLateTimer = TickTimer.CreateFromSeconds(Runner, reloadLateSecond);
                
                SoundManager.Play(reloadSound);
                var needChargingAmmoCount = magazine.Max - magazine.Current;
                
                if (ammo.Current < needChargingAmmoCount)
                    needChargingAmmoCount = ammo.Current;
                if (needChargingAmmoCount > bulletAmount)
                    needChargingAmmoCount = bulletAmount;
                
                DebugManager.Log($"탄약 충전 : {magazine.Current} + {needChargingAmmoCount}");

                ammo.Current -= needChargingAmmoCount;
                magazine.Current += needChargingAmmoCount;
                SetMagazineRPC(StatusValueType.Current, magazine.Current);
            }
        }

        #endregion

        #region Json Interface

        public GunJsonData GetJsonData()
        {
            throw new System.NotImplementedException();
        }
        
        public void SetJsonData(GunJsonData json)
        {
            name = json.Name;
            explain = json.Explain;
        }
        #endregion

        #region RPC Function

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetDestinationRPC(Vector3 dir) => bullet.destination = dir;

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void SetMagazineRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Current:
                    magazine.Current = value;
                    break;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ShootRPC() => Shoot();

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ReloadBulletRPC() => ReLoadBullet();

        #endregion
    }
}