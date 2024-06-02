using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Status;
using Fusion;
using Manager;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Util;
using Weapon.Bullet;

namespace Weapon.Gun
{
    public class GunBase : WeaponBase, IJsonData<GunJsonData>, IWeaponHitEffect
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
        
        // Overheating Material 캐싱
        private static readonly int Value = Shader.PropertyToID("_Heating_Multiple");
        
        #endregion
        
        private Camera _camera;
        private MeshRenderer _meshRenderer;

        [Header("총 정보")] 
        public int id;
        public string explain;

        [Header("총 이펙트")] 
        public VisualEffect shootEffect; // 발사 이펙트
        public VisualEffect shotsmoke;      // 총구 연기
        [SerializeField] private MaterialPropertyBlockExtension shotOverHeatingPropertyBlock;
        public NetworkPrefabRef hitEffectPrefab;
        
        [Header("사운드")]
        public AudioSource shootSound;
        public AudioSource emptyAmmoSound;
        public AudioSource reloadSound;

        [Header("총알")] 
        public BulletBase bullet;
        public static StatusValue<int> ammo = new StatusValue<int>(){Max = 100, Current = int.MaxValue};
        public StatusValue<int> magazine = new StatusValue<int>() {Max = 10, Current = 10}; // max 최대 탄약, current 현재 장정된 탄약

        public float bulletFirePerMinute; // 분당 총알 발사량
        public float BulletFirePerSecond => bulletFirePerMinute / 60f;
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
        public Action AfterFireAction;
        public Action AfterReloadAction;
        
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
            EquipAction += OverHeatCal;
            // EquipAction += SetVFX;
            ReleaseEquipAction += (obj) =>
            {
                AfterFireAction = null;  
                BeforeShootAction = null;
                status.ClearAdditionalStatus();
            };
            
            IsGun = true;
        }
        
        public override void Start()
        {
            base.Start();
            
            BulletInit();
        }

        public override void Spawned()
        {
            base.Spawned();
            
            AttackAction += () =>
            {
                if (FireLateTimer.Expired(Runner))
                {
                    FireLateTimer = TickTimer.CreateFromSeconds(Runner, fireLateSecond / status.CalAttackSpeed());
                    FireBulletRPC();
                }
            };
            FireLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
            ReloadLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }

        #endregion
        
        #region Member Funtion

        public virtual void BulletInit()
        {
            magazine.Current = int.MaxValue;
            
            // TODO : 여기서 공격속도 반영 해야함
            fireLateSecond = 60 / (bulletFirePerMinute);
        }
        
        private void SetCamera(GameObject equipObject)
        {
            if (equipObject.TryGetComponent(out PlayerCameraController pcc))
            {
                _camera = pcc.targetCamera;
            }
        }

        public virtual void FireBullet(bool isDst = true)
        {
            if (magazine.Current != 0)
            {
                var dst = fireTransform.forward * 2f;
                if (isDst)
                    dst = CheckRay();

                {   // Shot VFX & Effect
                    if (shootEffect != null)
                        MuzzleRPC();

                    if (shotsmoke != null)
                        SmokeRPC();
                }
                
                if (HasStateAuthority)
                {
                    int nuckBack = status.GetAllNuckBack();

                    Runner.SpawnAsync(bullet.gameObject, fireTransform.position, fireTransform.rotation, null,
                        (runner, o) =>
                        {
                            var b = o.GetComponent<BulletBase>();
                            b.OwnerId = OwnerId;
                            b.OwnerGunId = Object.Id;
                            b.KnockBack = nuckBack;
                            b.destination = fireTransform.position + (dst * status.attackRange);

                            BeforeShootAction?.Invoke(b);
                        });
                }

                --magazine.Current;

                SoundManager.Play(shootSound);

                AfterFireAction?.Invoke();
                DebugManager.Log($"{name}에서 총알을 발사");
            }
            else
            {
                SoundManager.Play(emptyAmmoSound);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulletAmount">장전 수 제한</param>
        /// <returns></returns>
        public int NeedReloadBulletCount(int bulletAmount = int.MaxValue)
        {
            var needChargingAmmoCount = magazine.Max - magazine.Current;
            if (ammo.Current < needChargingAmmoCount)
                needChargingAmmoCount = ammo.Current;
            if (needChargingAmmoCount > bulletAmount)
                needChargingAmmoCount = bulletAmount;

            return needChargingAmmoCount;
        }

        protected virtual void ReLoadBullet(int needChargingAmmoCount)
        {
            SoundManager.Play(reloadSound);

            if(HasInputAuthority)
                ammo.Current -= needChargingAmmoCount;
            magazine.Current += needChargingAmmoCount;
                
            OverHeatCal();
            AfterReloadAction?.Invoke();
            DebugManager.Log($"탄약 충전 : {magazine.Current} + {needChargingAmmoCount}");
        }

        public void ReloadBullet()
        {
            if (ReloadLateTimer.Expired(Runner) && ammo.isMin == false && HasInputAuthority)
            { 
                int needBulletCount = NeedReloadBulletCount();
                if (needBulletCount != 0)
                {
                    ReloadLateTimer = TickTimer.CreateFromSeconds(Runner, reloadLateSecond);
                    ReloadBulletRPC(needBulletCount);
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
        }

        private void SetVFX(GameObject gameObject)
        {
            shotOverHeatingPropertyBlock.Block.SetFloat(Value, 0.0f);
            shotOverHeatingPropertyBlock.SetBlock();
            // shotsmoke.gameObject.SetActive(false);
            
            DebugManager.ToDo("Muzzle Layer 설정 변경해야함");
            var muzzleTransform = transform.Find("Muzzle");
                
            if(null != muzzleTransform)
                muzzleTransform.gameObject.layer = LayerMask.NameToLayer("Weapon");
                
            var smokeTransform = transform.Find("Smoke");
                
            if(null != smokeTransform)
                smokeTransform.gameObject.layer = LayerMask.NameToLayer("Weapon");
        }
        
        // weaponSystem에서 작동해여 코루틴이 끝까지 작동함
        // TODO : 로직 수정으로 인해서 잠시 사용 중단
        IEnumerator OverHeatCoroutine()
        {
            float value;
        
            float elapsedTime = 0f;
            float duration = 0.6f; // 보간에 걸리는 시간
        
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                value = Mathf.Lerp(0, 0.8f, elapsedTime / duration);
                shotOverHeatingPropertyBlock.Block.SetFloat(Value, value);
                shotOverHeatingPropertyBlock.SetBlock();
                // shotOverHeating.SetFloat(Value, value);
                yield return null;
            }
        
            while (elapsedTime > 0.0f)
            {
                elapsedTime -= Time.deltaTime;
                value = Mathf.Lerp(0, 0.8f, elapsedTime / duration);
                shotOverHeatingPropertyBlock.Block.SetFloat(Value, value);
                shotOverHeatingPropertyBlock.SetBlock();
                // shotOverHeating.SetFloat(Value, value);
                yield return null;
            }
            // if(shotOverHeating != null)
            //     shotOverHeating.SetFloat(Value, 0.0f);
            // if(shotsmoke != null)
            //     shotsmoke.gameObject.SetActive(false);
        }
        
        void OverHeatCal()
        {
            float bulletCount = 1 - (float)magazine.Current / magazine.Max;
            
            StartCoroutine(OverHitTimer(shotOverHeatingPropertyBlock.Block.GetFloat(Value), bulletCount));
        }

        IEnumerator OverHitTimer(float LValue, float RValue)
        {            
            float amount;
            
            float elapsedTime = 0f;
            float duration = 0.6f; // 보간에 걸리는 시간
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                amount = Mathf.Lerp(LValue, RValue, elapsedTime / duration);
                shotOverHeatingPropertyBlock.Block.SetFloat(Value, amount);
                shotOverHeatingPropertyBlock.SetBlock();
                DebugManager.Log($"{amount}");
                yield return null;
            }
        }

        #endregion

        #region Weapon Interface

        public virtual void OnWeaponHitEffect(Vector3 hitPosition)
        {
            Runner.SpawnAsync(hitEffectPrefab, hitPosition);
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
        public void FireBulletRPC()
        {
            FireBullet();
            OverHeatCal();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ReloadBulletRPC(int needChargingAmmoCount) => ReLoadBullet(needChargingAmmoCount);

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void MuzzleRPC()
        {
            if(false == shootEffect.gameObject.activeSelf)
                shootEffect.gameObject.SetActive(true);
            shootEffect.SendEvent("OnPlay");
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SmokeRPC()
        {
            if (false == shotsmoke.gameObject.activeSelf)
                shotsmoke.gameObject.SetActive(true);
            shotsmoke.SendEvent("OnPlay");
            // StartCoroutine(OverHeatCoroutine());
        }
        
        #endregion
    }
}