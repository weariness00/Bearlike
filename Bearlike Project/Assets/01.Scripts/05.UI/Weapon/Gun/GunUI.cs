using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Weapon.Gun;
using DebugManager = Manager.DebugManager;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public GunBase gun;
        public TMP_Text bulletCount;
        public TMP_Text ammoCount;

        public Image amount;

        private int _magazineCount;

        private void Start()
        {
            amount.fillAmount = (float)gun.magazine.Current / (float)gun.magazine.Max;
            
            DebugManager.ToDo("총의 종류에 따라 Ammo를 받아오는 방식으로 변경해야함");
            ammoCount.text = GunBase.ammo.Current.ToString();
            
            bulletCount.text = gun.magazine.Current.ToString();
        }
        
        private void Update()
        {
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
