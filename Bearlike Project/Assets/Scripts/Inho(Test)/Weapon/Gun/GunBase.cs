using Fusion;
using Script.Manager;
using Scripts.State.GameStatus;
using State.StateClass.Base;
using State.StateSystem;
using UnityEngine;

namespace Inho_Test_.Weapon.Gun
{
    public class GunBase : WeaponBase
    {
        [Header("사운드")]
        public AudioSource shootSound;
        public AudioSource emptyAmmoSound;
        public AudioSource reloadSound;
        
        [Header("총알")]
        public StatusValue<int> magazine = new StatusValue<int>(); // 한 탄창
        public StatusValue<int> ammo = new StatusValue<int>(); // 총 탄약

        public virtual void Awake()
        {
            base.Awake();
        }
        
        public virtual void Start()
        {
            base.Start();
            AttackAction += Shoot;
            IsGun = true;

            BulletInit();
        }

        public virtual void Shoot()
        {
            if (magazine.Current != 0)
            {
                CheckRay();
                magazine.Current--;
                SoundManager.Play(shootSound);
            }
            else
            {
                SoundManager.Play(emptyAmmoSound);
            }
        }

        // 카메라가 바라보는 방향으로 직선 레이를 쏜다.    
        public void CheckRay()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
            if (Physics.Raycast(ray, out var hit))
            {
                DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.collider.name}");
                
                var hitStateSystem = hit.collider.GetComponent<StateSystem>();
                var hitState = hitStateSystem.GetState();

                if (hitState != null)
                {
                    hitState.ApplyDamage(state.attack.Current, (ObjectProperty)state.property); // 총의 공격력을 여기서 추가를 할지 아님 state에서 추가를 할지 고민해보자.
                    hitState.ShowInfo();
                }
            }
        }
        
        private void ApplyDamage(Hitbox playerHitbox)
        {
            var playerStateSystem = playerHitbox.Root.GetComponent<StateSystem>();
            var playerState = playerStateSystem.GetState();

            var monsterStateSystem = gameObject.GetComponent<StateSystem>();
            var monsterState = monsterStateSystem.GetState();
            
            playerState.ApplyDamage(monsterState.attack.Current, (ObjectProperty)monsterState.property);
        }

        #region Bullet Funtion

        public virtual void BulletInit()
        {
            magazine.Max = 10;
            magazine.Current = int.MaxValue;
        }
        
        public virtual void ReLoadBullet()
        {
            SoundManager.Play(reloadSound);
            var needChargingAmmoCount = magazine.Max - magazine.Current;
            if (ammo.Current < needChargingAmmoCount)
            {
                needChargingAmmoCount = ammo.Current;
            }
            
            magazine.Current += needChargingAmmoCount;
            ammo.Current -= needChargingAmmoCount;
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