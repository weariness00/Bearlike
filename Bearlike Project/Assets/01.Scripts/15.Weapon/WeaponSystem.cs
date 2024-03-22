using Fusion;
using Script.Weapon.Gun;
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
            
        }

        private void Update()
        {
            
        }
    }
}