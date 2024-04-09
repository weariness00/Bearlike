using TMPro;
using UnityEngine;
using Weapon.Gun;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public GunBase gun;
        public TMP_Text bulletCurrentText;
        public TMP_Text bulletMaxText;
        
        public int bulletMaxCount;
        private int _magazineCount;

        private void Start()
        {
            bulletMaxCount = _magazineCount = gun.magazine.Max;
            
            bulletMaxText.text = gun.magazine.Max.ToString();
            bulletCurrentText.text = gun.magazine.Current.ToString();
        }
        
        private void Update()
        {
            if (_magazineCount != gun.magazine.Current)
            {
                bulletCurrentText.text = gun.magazine.Current.ToString();
                _magazineCount = gun.magazine.Current;
            }
        }
    }
}
