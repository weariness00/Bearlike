using Fusion;
using Inho_Test_.Player;
using Manager;
using State.StateClass.Base;
using Status;
using UnityEngine;
using UnityEngine.VFX;
using Weapon;
using Weapon.Bullet;

namespace Script.Weapon.Gun
{
    public class GunBase : WeaponBase
    {
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
        public StatusValue<float> fireLateSecond = new StatusValue<float>(); // 총 발사후 기다리는 시간
        public StatusValue<float> reloadLateSecond = new StatusValue<float>(){Max = 1, Current = float.MaxValue}; // 재장전 시간

        public float attackRange;       // 총알 사정거리

        [Header("성능")] 
        public StatusValue<int> attack = new StatusValue<int>();     // 공격력
        public int property;                // 속성
        
        public override void Awake()
        {
            base.Awake();
        }
        
        public override void Start()
        {
            base.Start();
            AttackAction += Shoot;
            IsGun = true;
            

            BulletInit();
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsPlayer == false)
            {
                return;
            }
            
            base.FixedUpdateNetwork();
            fireLateSecond.Current += Runner.DeltaTime;
            reloadLateSecond.Current += Runner.DeltaTime;
        }

        public virtual void Shoot()
        {
            if (reloadLateSecond.isMax && fireLateSecond.isMax)
            {
                fireLateSecond.Current = fireLateSecond.Min;
                if (magazine.Current != 0)
                {
                    var dst = CheckRay();
                    
                    if(shootEffect != null) shootEffect.Play();
                    bullet.destination = dst;
                    bullet.hitEffect = hitEffect;
                    bullet.bknock = false;
                    
                    var transform1 = transform;
                    Instantiate(bullet.gameObject, transform1.position, transform1.rotation);
                
                    magazine.Current--;
                    SoundManager.Play(shootSound);
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
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            DebugManager.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
            
            return ray.direction;
            
            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            if(Runner.LagCompensation.Raycast(ray.origin, ray.direction, float.MaxValue, Object.InputAuthority, out var hit, includeCollide, hitOptions))
            {
                DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.GameObject.name}");

                StatusBase hitState;
                if (hit.GameObject.TryGetComponent(out hitState))
                {
                    // hitState.ApplyDamage(state.attack.Current, (ObjectProperty)state.property); // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
                    // hitState.ApplyDamageRPC(status.attack.Current, (CrowdControl)status.property);
                    var hitEffectObject = Instantiate(hitEffect.gameObject, hit.Point, Quaternion.identity);
                    hitEffectObject.transform.LookAt(hit.Normal);
                    Destroy(hitEffectObject, 5f);
                }
                
                detination = hit.Point;
            }
            else
            {
                detination = ray.direction * int.MaxValue;
            }

            return detination;
        }
        
        // public virtual void ApplyDamage(Hitbox enemyHitbox)
        // {
        //     var enemyState = enemyHitbox.Root.GetComponent<StatusBase>();
        //
        //     if (enemyState.gameObject.CompareTag("Player"))
        //     {
        //         return;
        //     }
        //     
        //     if (enemyState == null || enemyState.hp.isMin)
        //     {
        //         return;
        //     }
        //     
        //     float damageMultiplier = enemyHitbox is TestBodyHitbox bodyHitbox ? bodyHitbox.damageMultiplier : 1f;
        //     
        //     // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
        //     enemyState.ApplyDamageRPC((int)((status.attack.Current + attack.Current) * damageMultiplier), (CrowdControl)(status.property | property));
        // }

        #region Bullet Funtion

        public virtual void BulletInit()
        {
            magazine.Max = 10;
            magazine.Current = int.MaxValue;

            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;

            attackRange = 100.0f;
            
            bullet.maxMoveDistance = attackRange;
            bullet.player = gameObject;
        }
        
        public virtual void ReLoadBullet()
        {
            if (reloadLateSecond.isMax && ammo.isMin == false)
            {
                reloadLateSecond.Current = reloadLateSecond.Min;
                
                SoundManager.Play(reloadSound);
                var needChargingAmmoCount = magazine.Max - magazine.Current;
                if (ammo.Current < needChargingAmmoCount)
                {
                    needChargingAmmoCount = ammo.Current;
                }
            
                magazine.Current += needChargingAmmoCount;
                ammo.Current -= needChargingAmmoCount;
            }
        }

        #endregion

        #region Equip

        // public Action AttackAction { get; set; }
        // public Action EquipAction { get; set; }
        // public bool IsEquip { get; set; }
        // public bool IsGun { get; set; }

        public override void Equip()
        {
            base.Equip();
            EquipAction?.Invoke();
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetDestinationRPC(Vector3 dir) => bullet.destination = dir;

        #endregion
    }
}