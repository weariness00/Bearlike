using UnityEngine;

namespace Weapon.Gun.Container
{
    public class HunterSniper : Sniper
    {
        public HunterSniperAnimator animatorInfo;

        public override void Start()
        {
            base.Start();
            AfterFireAction += () =>
            {
                animatorInfo.SetFireSpeed(fireLateSecond);
                animatorInfo.PlayFire();
            };

            AfterReloadAction += () =>
            {
                animatorInfo.SetReloadSpeed(reloadLateSecond);
                animatorInfo.PlayReload();
            };
        }
    }
}

