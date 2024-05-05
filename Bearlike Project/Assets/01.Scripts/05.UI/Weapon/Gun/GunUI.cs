using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Weapon.Gun;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public GunBase gun;
        // public TMP_Text bulletCurrentText;
        // public TMP_Text bulletMaxText;
        public TMP_Text bulletCount;
        public TMP_Text ammoCount;

        public Image amount;

        private int _magazineCount;

        private void Start()
        {
            amount.fillAmount = (float)gun.magazine.Current / (float)gun.magazine.Max;
            
            ammoCount.text = gun.ammo.Current.ToString();
            
            // bulletMaxText.text = gun.magazine.Max.ToString();
            // bulletCurrentText.text = gun.magazine.Current.ToString();
            // bulletCount.text = gun.magazine.Current + " / " + gun.magazine.Max;
        }
        
        private void Update()
        {
            if (_magazineCount != gun.magazine.Current)
            {
                // bulletCount.text = gun.magazine.Current + " / " + gun.magazine.Max;
                
                amount.fillAmount = (float)gun.magazine.Current / (float)gun.magazine.Max;
                
                _magazineCount = gun.magazine.Current;
                ammoCount.text = gun.ammo.Current.ToString();
            }
        }
    }
}
