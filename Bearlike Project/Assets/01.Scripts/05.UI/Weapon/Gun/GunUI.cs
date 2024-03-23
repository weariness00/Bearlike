using Script.Weapon.Gun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        public GameObject UI;
        
        public int bulletMaxCount;

        private GunBase _gun;
        private TextMeshProUGUI _bulletText;

        private void Start()
        {
            _bulletText = UI.GetComponent<TextMeshProUGUI>();
            _gun = transform.root.GetComponentInChildren<GunBase>();
            // _gun = GetComponentInParent<GunBase>();
            bulletMaxCount = _gun.magazine.Max;
        }

        private void Update()
        {
            _bulletText.text = _gun.magazine.Current.ToString() + " / " + bulletMaxCount.ToString();
        }
    }
}
