using System;
using Script.Manager;
using UnityEngine;

namespace Script.Weapon.Gun
{
    public class GunBase : WeaponBase, IEquipment
    {
        protected virtual void Start()
        {
            Action += CheckRay;
        }

        public virtual void Shoot()
        {
            
        }

        public void CheckRay()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
            if (Physics.Raycast(ray, out var hit))
            {
                DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.collider.name}");
            }
        }

        #region Equip

        public Action Action { get; set; }
        public bool IsEquip { get; set; }

        public void Equip()
        {
            
        }

        #endregion
    }
}

