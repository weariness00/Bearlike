using System;
using Fusion;
using Script.Weapon.Gun;

namespace Weapon.Gun
{
    public class GunSystem : NetworkBehaviour
    {
        public GunBase gun;     // 소유한 무기

        private void Awake()
        {
            // gun = gameObject.AddComponent<Maganum>();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
    }
}