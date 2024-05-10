using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Weapon;
using Weapon.Gun;
using DebugManager = Manager.DebugManager;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public WeaponSystem weaponSystem;
        public TMP_Text bulletCount;
        public TMP_Text ammoCount;

        public Image amount;

        private int _magazineCount;

        private void Start()
        {
            SetUI();
        }
        
        private void Update()
        {
            DebugManager.ToDo("Gun만다루는 UI가 아닌 Weapon 전체를 다루는 UI로 꾸바기");
            GunUpdate();
        }

        private void SetUI()
        {
            if (weaponSystem.equipment.IsGun)
            {
                var gun = weaponSystem.equipment as GunBase;
                amount.fillAmount = (float)gun.magazine.Current / (float)gun.magazine.Max;
            
                DebugManager.ToDo("총의 종류에 따라 Ammo를 받아오는 방식으로 변경해야함");
                ammoCount.text = GunBase.ammo.Current.ToString();
            
                bulletCount.text = gun.magazine.Current.ToString();
            }
        }

        private void GunUpdate()
        {
            if (weaponSystem.equipment.IsGun)
            {
                var gun = weaponSystem.equipment as GunBase;
                if (_magazineCount != gun.magazine.Current)
                {
                    bulletCount.text = gun.magazine.Current.ToString();
                
                    amount.fillAmount = (float)gun.magazine.Current / (float)gun.magazine.Max;
                
                    _magazineCount = gun.magazine.Current;
                    ammoCount.text = GunBase.ammo.Current.ToString();
                }
            }
        }
    }
}
