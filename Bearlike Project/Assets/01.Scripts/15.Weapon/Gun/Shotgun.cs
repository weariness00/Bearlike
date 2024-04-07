using System.Collections;
using Script.Weapon.Gun;
using State.StateClass.Base;
using UnityEngine;

namespace Weapon.Gun
{
    public class Shotgun : GunBase
    {
        [SerializeField] private float reloadSpeed = 0.5f;  // 재장전 속도

        [SerializeField] private float bulletRadian;    // 산탄 정도(원의 반지름)
        public bool bknock;

        private IEnumerator _reloadCorutine;
        
        public override void Awake()
        {
            base.Awake();            
            
            BulletInit();
        }
        
        public override void Start()
        {
            base.Start();

            bknock = false;
            bulletRadian = 3;
            
            ammo.Max = 36;
            ammo.Current = ammo.Max;
            
            attack.Max = 5;
            attack.Current = attack.Max;
            property = (int)CrowdControl.Normality;
        } 
        
        public override void Shoot()
        {                        
            // 다른 클라가 했으면 자기 말고 다른 클라의 코드를 실행해야지
            if (fireLateSecond.isMax)
            {
                fireLateSecond.Current = fireLateSecond.Min;
                if (magazine.Current != 0)
                {
                    var dst = CheckRay();
                    
                    if(shootEffect != null) shootEffect.Play();
                    bullet.hitEffect = hitEffect;
                    
                    bullet.bknock = bknock;
                    
                    for (int i = 0; i < 10; ++i)
                    {
                        Vector3 randomVector3 = new Vector3(Random.Range(-bulletRadian, bulletRadian), 
                            Random.Range(-bulletRadian, bulletRadian), Random.Range(-bulletRadian, bulletRadian));

                        // SetDestinationRPC((dst * attackRange) + randomVector3);
                        bullet.destination = transform.position + (dst * attackRange) + randomVector3;
                        
                        var transform1 = transform;
                        Runner.SpawnAsync(bullet.gameObject, transform1.position + dst, transform1.rotation);
                    }
                 
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
            bulletFirePerMinute = 80;
            
            magazine.Max = 5;
            magazine.Current = magazine.Max;
            
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