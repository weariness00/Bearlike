using System.Collections;
using Fusion;
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

            ammo.Max = ammo.Current = 40;
        } 
        
        public override void Shoot()
        {
            base.Shoot();
            // 코드의 재사용을 해야됨
            // if (fireLateSecond.isMax)
            // {
            //     fireLateSecond.Current = fireLateSecond.Min;
            //     if (magazine.Current != 0)
            //     {
            //         var dst = CheckRay();
            //         
            //         if(shootEffect != null) shootEffect.Play();
            //         bullet.destination = dst;
            //         
            //         var transform1 = transform;
            //         Instantiate(bullet.gameObject, transform1.position, transform1.rotation);
            //     
            //         magazine.Current--;
            //         SoundManager.Play(shootSound);
            //     }
            //     else
            //     {
            //         SoundManager.Play(emptyAmmoSound);
            //     }
            // }
            if(_reloadCorutine != null)
                StopCoroutine(_reloadCorutine);
        }
        
        #region Bullet Funtion
        
        public override void ReLoadBullet(int bulletAmount = int.MaxValue)
        {
            if (ReloadLateTimer.Expired(Runner) && ammo.isMin == false)
            {
                ReloadLateTimer = TickTimer.CreateFromSeconds(Runner, reloadLateSecond);
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