using System.Collections;
using Script.Weapon.Gun;
using State.StateClass.Base;
using UnityEngine;

namespace Weapon.Gun
{
    public class Shotgun : GunBase
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

            ammo.Max = ammo.Current = 36;
            bulletFirePerMinute = 60;
            
            attack.Max = attack.Current = 3;
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
                    
                    // gun의 현재 방향에서 원을 그리는 벡터를 dst에 더하면?
                    // 벡터에 원하는 원의 반지름을 곱하면 그만큼 탄의 퍼짐이 결정이된다.
                    
                    // 아니면 일정거리는 모든탄이 날아가다가 일정 구간에서 퍼지는 (우클릭? 혹은 일반 공격)으로 구현해도 괜찬을것 같다.
                    
                    if(shootEffect != null) shootEffect.Play();
                    bullet.destination = dst;
                    
                    Instantiate(bullet.gameObject, transform.position, transform.rotation);
                
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
            magazine.Max = magazine.Current = 5;
            
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