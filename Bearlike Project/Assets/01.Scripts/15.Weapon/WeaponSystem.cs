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
            equipment = GetComponentInChildren<WeaponBase>();

            weaponList = GetComponentsInChildren<WeaponBase>().ToList();
        }

        public void ChangeEquipment(int index, GameObject equipTargetObject)
        {
            // 장비 해제
            equipment.ReleaseEquipAction?.Invoke(equipTargetObject);
            
            // 장비 변경
            equipment = weaponList.Count > index ? weaponList.First() : weaponList[index];
            
            // 변경한 장비를 착용
            equipment.EquipAction?.Invoke(equipTargetObject);
        }
    }
}