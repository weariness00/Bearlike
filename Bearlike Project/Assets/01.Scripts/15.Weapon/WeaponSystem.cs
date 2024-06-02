﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Weapon.Gun;

namespace Weapon
{
    public class WeaponSystem : NetworkBehaviour
    {
        public IEquipment equipment; // 현재 장착 중인 무기

        public List<WeaponBase> weaponList;
        public GameObject smokeObject;

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
            
            if (HasInputAuthority)
            {
                Transform[] children = new Transform[smokeObject.transform.childCount];
                for (int i = 0; i < smokeObject.transform.childCount; i++)
                {
                    children[i] = smokeObject.transform.GetChild(i);
                }

                foreach (var child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Weapon");
                }
            }
        }

        public bool ChangeEquipment(int index, GameObject equipTargetObject)
        {
            if(weaponList.Count < index)  return false;
            if((WeaponBase)equipment == weaponList[index]) return false;
            
            // 장비 해제
            equipment.ReleaseEquipAction?.Invoke(equipTargetObject);
            
            // 장비 변경
            equipment = weaponList[index];
            
            // 변경한 장비를 착용
            equipment.EquipAction?.Invoke(equipTargetObject);
            if (equipment.IsGun && equipment is GunBase gun)
                gun.OverHeatCal();
            
            return true;
        }
    }
}