using System.Collections;
using Fusion;
using UnityEngine;
using Weapon.Gun.Continer;

namespace Weapon.Gun
{
    public class Revolver : GunBase
    {
        public RevolverAnimator animatorInfo;

        public override void Awake()
        {
            base.Awake();

            EquipAction += (targetObject) =>
            {
                AfterFireAction += () =>
                {
                    if(playerCameraController) playerCameraController.ReboundCamera();

                    animatorInfo.PlayFire();
                    animatorInfo.SetFireSpeed(fireLateSecond);
                };

                AfterReloadAction += () =>
                {
                    animatorInfo.PlayReload();
                    animatorInfo.SetReloadSpeed(reloadLateSecond);
                };
            };
        }
    }
}