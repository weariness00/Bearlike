using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Weapon
{
    public class WeaponSystem : NetworkBehaviour
    {
        public IEquipment equipment; // 현재 장착 중인 무기

        public List<WeaponBase> weaponList;

        private void Awake()
        {
            var equipWeapon = GetComponentInChildren<WeaponBase>();
            equipment = equipWeapon;
        }

        private void Start()
        {
            weaponList = GetComponentsInChildren<WeaponBase>().ToList();
            foreach (var weapon in weaponList)
                weapon.gameObject.SetActive(false);
            ((WeaponBase)equipment).gameObject.SetActive(true);
        }

        public void ChangeEquipment(int index, GameObject equipTargetObject)
        {
            if(weaponList.Count < index)
                return;
            if((WeaponBase)equipment == weaponList[index])
                return;
            
            // 장비 해제
            equipment.ReleaseEquipAction?.Invoke(equipTargetObject);
            
            // 장비 변경
            equipment = weaponList[index];
            
            // 변경한 장비를 착용
            equipment.EquipAction?.Invoke(equipTargetObject);
        }
    }
}