using UnityEngine;

namespace Weapon.Gun.Container
{
    public class HunterSniper : GunBase
    {
        public HunterSniperAnimator animatorInfo;

        public override void Awake()
        {
            base.Awake();
            EquipAction += (targetObject) =>
            {
                AfterFireAction += () =>
                {
                    if(playerCameraController) playerCameraController.ReboundCamera();

                    animatorInfo.SetFireSpeed(fireLateSecond);
                    animatorInfo.PlayFire();
                };

                AfterReloadAction += () =>
                {
                    animatorInfo.SetReloadSpeed(reloadLateSecond);
                    animatorInfo.PlayReload();
                };
            };
        }
    }
}

