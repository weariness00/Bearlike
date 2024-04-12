using System.Collections;
using UnityEngine;

namespace Weapon.Gun
{
    public class Magnum : GunBase
    {
        [SerializeField] private float reloadSpeed = 0.5f;

        private IEnumerator _reloadCorutine;
        
        public override void Awake()
        {
            base.Awake();
            
            BulletInit();
        } 
        
        public override void Start()
        {
            base.Start();

            // json화
            ammo.Max = ammo.Current = 40;
        } 
        
        public override void Shoot()
        {
            base.Shoot();
            
            if(_reloadCorutine != null)
                StopCoroutine(_reloadCorutine);
        }
        
        #region Bullet Funtion
        
        public override void ReLoadBullet(int bulletAmount = int.MaxValue)
        {
            if (reloadLateSecond.isMax && ammo.isMin == false)
            {
                reloadLateSecond.Current = reloadLateSecond.Min;
                
                var needChargingAmmoCount = magazine.Max - magazine.Current;
                
                if (ammo.Current < needChargingAmmoCount)
                {
                    needChargingAmmoCount = ammo.Current;
                }

                _reloadCorutine = ReloadCorutine(reloadSpeed, needChargingAmmoCount);
                StartCoroutine(_reloadCorutine);
            }
        }

        IEnumerator ReloadCorutine(float waitTime, int repeatCount)
        {
            for (int i = 0; i < repeatCount; ++i)
            {
                base.ReLoadBullet(1);
                yield return new WaitForSeconds(waitTime);
            }
        }

        #endregion
    }
}