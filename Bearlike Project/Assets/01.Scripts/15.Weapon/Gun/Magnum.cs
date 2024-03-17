using System.Collections;
using Script.Weapon.Gun;
using State.StateClass.Base;
using Status;
using UnityEngine;
using Weapon.Bullet;

namespace Weapon.Gun
{
    public class Magnum : GunBase
    {
        [SerializeField] private float _reloadSpeed = 0.5f;
        
        public override void Awake()
        {
            base.Awake();
        } 
        
        public override void Start()
        {
            base.Start();

            ammo.Max = ammo.Current = 36;
            bulletFirePerMinute = 120;
            
            attack.Max = attack.Current = 10;
            property = (int)CrowdControl.Normality;
            
            BulletInit();
        } 
        
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            base.BulletInit();
            magazine.Max = magazine.Current = 6;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
            
            reloadLateSecond.Max = reloadLateSecond.Current = 0.5f;
        }
        
        public override void ReLoadBullet()
        {
            if (reloadLateSecond.isMax && ammo.isMin == false)
            {
                reloadLateSecond.Current = reloadLateSecond.Min;
                
                var needChargingAmmoCount = magazine.Max - magazine.Current;
                
                if (ammo.Current < needChargingAmmoCount)
                {
                    needChargingAmmoCount = ammo.Current;
                }

                StartCoroutine(ReloadCorutine(_reloadSpeed, needChargingAmmoCount));
                
                // for (int i = 0; i < needChargingAmmoCount; ++i)
                // {
                //     SoundManager.Play(reloadSound);
                //     magazine.Current += 1;
                //     ammo.Current -= 1;
                // }
            }
        }

        IEnumerator ReloadCorutine(float waitTime, int repeatCount)
        {
            for (int i = 0; i < repeatCount; ++i)
            {
                SoundManager.Play(reloadSound);
                magazine.Current += 1;
                ammo.Current -= 1;
                yield return new WaitForSeconds(waitTime);
            }
            yield break;
        }

        #endregion
    }
}