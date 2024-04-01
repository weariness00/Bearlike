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

        private IEnumerator _reloadCorutine;
        
        public override void Awake()
        {
            base.Awake();
        }
        
        public override void Start()
        {
            base.Start();

            bulletRadian = 3;
            
            ammo.Max = ammo.Current = 36;
            bulletFirePerMinute = 80;
            
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
                    // dst에 구를 그릴수 있는 vector3를 뽑아서 방향만 설정하면 탄은 결과적으로 원을 그릴것이다.
                    // ==> 문제점이 반환되는 dst가 객체의 유무에 따라 distance의 길이가 달라지기에 일관성 없는 탄퍼짐이 나온다
                    // 그러면 사정거리에 따른 dst가 return이 되도록 raycast함수를 수정하던지 아니면 반환받은 dst를 표준화한뒤 사정거리를 곱하자.
                    
                    // 아니면 일정거리는 모든탄이 날아가다가 일정 구간에서 퍼지는 (우클릭? 혹은 일반 공격)으로 구현해도 괜찬을것 같다.
                    
                    if(shootEffect != null) shootEffect.Play();
                    
                    // dst = Vector3.Normalize(dst);    // 어차피 Ray의 반환값은 정규화가 되어있다.
                    // bullet.destination = dst * attackRange;  // 아래 for문에서 해준다.
                    
                    for (int i = 0; i < 10; ++i)
                    {
                        Vector3 randomVector3 = new Vector3(Random.Range(-bulletRadian, bulletRadian), 
                            Random.Range(-bulletRadian, bulletRadian), Random.Range(-bulletRadian, bulletRadian));
                        
                        bullet.destination = (dst * attackRange) + randomVector3 * bulletRadian;
                        
                        var transform1 = transform;
                        Instantiate(bullet.gameObject, transform1.position, transform1.rotation);
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
            magazine.Max = magazine.Current = 5;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
            
            reloadLateSecond.Max = reloadLateSecond.Current = 0.5f;

            attackRange = 300.0f;
            
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