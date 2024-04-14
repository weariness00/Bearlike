using TMPro;
using UnityEngine;
using Weapon.Gun;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public GunBase gun;
        // public TMP_Text bulletCurrentText;
        // public TMP_Text bulletMaxText;
        public TMP_Text bulletCount;
        
        private int _bulletMaxCount;
        private int _magazineCount;

        private void Start()
        {
            _bulletMaxCount = _magazineCount = gun.magazine.Max;
            
            // bulletMaxText.text = gun.magazine.Max.ToString();
            // bulletCurrentText.text = gun.magazine.Current.ToString();
            bulletCount.text = gun.magazine.Current + " / " + gun.magazine.Max;
        }
        
        private void Update()
        {
            if (_magazineCount != gun.magazine.Current)
            {
                bulletCount.text = gun.magazine.Current + " / " + gun.magazine.Max;
                _magazineCount = gun.magazine.Current;
            }
        }
    }
}
