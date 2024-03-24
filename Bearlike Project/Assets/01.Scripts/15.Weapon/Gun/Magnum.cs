using System.Collections;
using Fusion;
using Inho_Test_.Player;
using Script.Weapon.Gun;
using State.StateClass.Base;
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
        } 
        
        public override void Start()
        {
            base.Start();

            ammo.Max = ammo.Current = 40;
            bulletFirePerMinute = 120;
            
            attack.Max = attack.Current = 10;
            property = (int)CrowdControl.Normality;
            
            BulletInit();
        } 
        
        public override void Shoot()
        {
            if (fireLateSecond.isMax)
            {
                fireLateSecond.Current = fireLateSecond.Min;
                if (magazine.Current != 0)
                {
                    var dst = CheckRay();
                    
                    if(shootEffect != null) shootEffect.Play();
                    bullet.destination = dst;
                    
                    var transform1 = transform;
                    Instantiate(bullet.gameObject, transform1.position, transform1.rotation);
                
                    magazine.Current--;
                    SoundManager.Play(shootSound);
                }
                else
                {
                    SoundManager.Play(emptyAmmoSound);
                }
            }
            if(_reloadCorutine != null)
                StopCoroutine(_reloadCorutine);
        }
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            magazine.Max = magazine.Current = 8;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
            
            reloadLateSecond.Max = reloadLateSecond.Current = 0.5f;

            attackRange = 30.0f;
            
            bullet.maxMoveDistance = attackRange;
            bullet.player = gameObject;
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

                _reloadCorutine = ReloadCorutine(reloadSpeed, needChargingAmmoCount);
                
                StartCoroutine(_reloadCorutine);
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