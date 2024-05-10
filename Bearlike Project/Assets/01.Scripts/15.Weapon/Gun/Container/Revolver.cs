using System.Collections;
using Fusion;
using UnityEngine;
using Weapon.Gun.Continer;

namespace Weapon.Gun
{
    public class Revolver : GunBase
    {
        public RevolverAnimator animatorInfo;

        public override void Start()
        {
            base.Start();

            AfterFireAction += () =>
            {
                animatorInfo.PlayFire();
                animatorInfo.SetFireSpeed(fireLateSecond);
            };

            AfterReloadAction += () =>
            {
                animatorInfo.PlayReload();
                animatorInfo.SetReloadSpeed(reloadLateSecond);
            };
        }
    }
}