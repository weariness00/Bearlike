using Fusion;
using Unity.VisualScripting;
using Weapon.Gun;

namespace Weapon
{
    public class WeaponSystem : NetworkBehaviour
    {
        public GunBase gun;     // 소유한 무기
        
        private void Awake()
        {
            gun = gameObject.GetOrAddComponent<Shotgun>();
        }

        private void Start()
        {
            // GunUI 스크립트에서 활성화 하도록 바꿈
            // ui = GetComponentInChildren<GunUI>().gameObject.SetActive(true);
        }

        private void Update()
        {
            
        }
    }
}