using System;
using Script.GameStatus;
using Script.Manager;
using Script.Util;
using UnityEngine;

namespace Script.Weapon.Gun
{
    public class GunBase : WeaponBase, IEquipment
    {
        public StatusValue magazine = new StatusValue(); // 한 탄창
        public StatusValue ammo; // 총 탄약

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
                var hitStatus = hit.collider.GetComponent<Status>();

                if (hitStatus != null)
                {
                    hitStatus.hp.Current -= status.damage.Current;
                }
            }
        }

        #region Bullet Funtion

        public virtual void BulletInit()
        {
            magazine.Max = 10;
            magazine.Current = int.MaxValue;
        }
        
        public virtual void ReLoadBullet()
        {
            var needChargingAmmoCount = magazine.Max - magazine.Current;
            if (ammo.Current < needChargingAmmoCount) needChargingAmmoCount = ammo.Current;
            magazine.Current += needChargingAmmoCount;
            ammo.Current -= needChargingAmmoCount;
        }
        

        #endregion

        #region Equip

        public Action AttackAction { get; set; }
        public Action EquipAction { get; set; }
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }

        public void Equip()
        {
            EquipAction?.Invoke();
        }

        #endregion
    }
}