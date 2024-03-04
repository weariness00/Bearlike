using System;
using Fusion;
using Inho_Test_.Player;
using Script.Manager;
using Scripts.State.GameStatus;
using State;
using State.StateClass;
using State.StateClass.Base;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Weapon.Bullet;

namespace Script.Weapon.Gun
{
    public class GunBase : WeaponBase
    {
        public static StatusValue<int> ammo = new StatusValue<int>(){Max = 100, Current = int.MaxValue};

        [Header("이펙트")]
        
        [Header("사운드")]
        public AudioSource shootSound;
        public AudioSource emptyAmmoSound;
        public AudioSource reloadSound;

        [Header("총알")] 
        public BulletBase bullet;
        public StatusValue<int> magazine = new StatusValue<int>() {Max = 10, Current = 10}; // max 최대 탄약, current 현재 장정된 탄약

        public float bulletFirePerMinute; // 분당 총알 발사량
        public StatusValue<float> fireLateSecond = new StatusValue<float>(); // 총 발사후 기다리는 시간
        public StatusValue<float> reloadLateSecond = new StatusValue<float>(){Max = 1, Current = float.MaxValue}; // 재장전 시간

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
            if (fireLateSecond.isMax == false)
            {
                fireLateSecond.Current += Runner.DeltaTime;
            }
            if (reloadLateSecond.isMax == false)
            {
                reloadLateSecond.Current += Runner.DeltaTime;
            }
        }

        public virtual void Shoot()
        {
            if (fireLateSecond.isMax)
            {
                fireLateSecond.Current = fireLateSecond.Min;
                
                if (magazine.Current != 0)
                {
                    fireLateSecond.Current = fireLateSecond.Min;
                
                    var dst = CheckRay();

                    bullet.destination = dst;
                    Instantiate(bullet.gameObject, transform.position, transform.rotation);
                
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
            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            DebugManager.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
            if(Runner.LagCompensation.Raycast(ray.origin, ray.direction, float.MaxValue, Object.InputAuthority, out var hit, Int32.MaxValue, hitOptions))
            {
                DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.GameObject.name}");
                
                var hitbox = hit.Hitbox;
                if (hitbox == null)
                {
                    // if (hit.GameObject.CompareTag("Destruction"))
                    // {
                    //     MeshDestruction.Destruction(hit.GameObject, PrimitiveType.Cube, hit.Point, Vector3.one * 2, ray.direction);
                    // }
                }
                else
                {
                    var hitState = hitbox.Root.GetComponent<StatusBase>();
                    if (hitState != null)
                    {
                        // hitState.ApplyDamage(state.attack.Current, (ObjectProperty)state.property); // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
                        ApplyDamage(hitbox);
                    }
                }
                detination = hit.Point;
            }
            else
            {
                detination = ray.direction * int.MaxValue;
            }

            return detination;
        }
        
        private void ApplyDamage(Hitbox enemyHitbox)
        {
            var enemyState = enemyHitbox.Root.GetComponent<StatusBase>();

            if (enemyState.gameObject.CompareTag("Player"))
            {
                return;
            }
            
            if (enemyState == null || enemyState._hp.isMin)
            {
                return;
            }
            
            float damageMultiplier = enemyHitbox is TestBodyHitbox bodyHitbox ? bodyHitbox.damageMultiplier : 1f;
            
            // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
            enemyState.ApplyDamageRPC((status.attack.Current + attack.Current) * damageMultiplier, (CrowdControl)status.property);
        }

        #region Bullet Funtion

        public virtual void BulletInit()
        {
            magazine.Max = 10;
            magazine.Current = int.MaxValue;

            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
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
    }
}