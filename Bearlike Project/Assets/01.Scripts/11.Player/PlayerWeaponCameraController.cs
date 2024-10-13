using System;
using UnityEngine;
using Weapon;
using Weapon.Gun;

namespace Player
{
    public class PlayerWeaponCameraController : MonoBehaviour
    {
        [SerializeField] private Camera weaponCamera;
        
        [SerializeField] private Vector3 oneHandOffset;
        [SerializeField] private Vector3 twoHandOffset;

        public void ChangeType(IEquipment equipment)
        {
            if (equipment.IsGun && equipment is GunBase gun)
            {
                switch (gun.handType)
                {
                    case GunBase.GunHandType.OneHand:
                        weaponCamera.transform.localPosition = oneHandOffset;
                        break;
                    case GunBase.GunHandType.TwoHand:
                        weaponCamera.transform.localPosition = twoHandOffset;
                        break;
                }
            }
        }
    }
}

