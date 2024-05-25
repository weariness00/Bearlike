using System.Collections;
using Fusion;
using Manager;
using UnityEngine;
using Weapon.Bullet;

namespace Weapon.Gun.Continer
{
    public class Shotgun : GunBase
    {
        [Header("Shotgun Information")]
        public MegaShotGunAnimator animatorInfo;

        [SerializeField] private float bulletRadian; // 산탄 정도(원의 반지름)

        private Coroutine _reloadCoroutine;

        public override void Awake()
        {
            base.Awake();

            BulletInit();
        }

        public override void Start()
        {
            base.Start();
            
            // json화
            ammo.Max = 36;
            ammo.Current = ammo.Max;
        }
        
        public override void FireBullet(bool isDst = true)
        {
            if (magazine.Current != 0)
            {
                if(playerCameraController) playerCameraController.ReboundCamera();
                animatorInfo.SetFireSpeed(BulletFirePerSecond);
                animatorInfo.PlayFireBullet();
                
                MuzzleRPC();
                
                var dst = CheckRay();

                if (shootEffect != null) shootEffect.Play();
                if (HasStateAuthority)
                {
                    int nuckBack = status.GetAllNuckBack();
                    
                    for (int i = 0; i < 10; ++i)
                    {
                        Runner.SpawnAsync(bullet.gameObject, transform.position + dst, transform.rotation, null,
                            (runner, o) =>
                            {
                                Vector3 randomVector3 = new Vector3(Random.Range(-bulletRadian, bulletRadian),
                                    Random.Range(-bulletRadian, bulletRadian), Random.Range(-bulletRadian, bulletRadian));

                                var b = o.GetComponent<BulletBase>();
                                b.OwnerId = OwnerId;
                                b.HitEffectId = Object.Id;
                                b.KnockBack = nuckBack;
                                // b.status.attackRange.Max = status.attackRange.Max;
                                // b.status.attackRange.Current = status.attackRange.Current;
                                b.destination = fireTransform.position + (dst * status.attackRange) + randomVector3;
                                BeforeShootAction?.Invoke(b);
                            });
                    }
                }

                --magazine.Current;
                // if (HasStateAuthority)
                //     SetMagazineRPC(StatusValueType.Current, magazine.Current);
                SoundManager.Play(shootSound);
                DebugManager.Log($"{name}에서 총알을 발사");
            }
            else
            {
                SoundManager.Play(emptyAmmoSound);
            }
            if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        }

        #region Bullet Funtion

        protected override void ReLoadBullet(int needChargingAmmoCount)
        {
            if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = StartCoroutine(ReloadCoroutine(needChargingAmmoCount));
        }

        IEnumerator ReloadCoroutine(int repeatCount)
        {
            animatorInfo.PlayReloadStart();
            yield return new WaitForSeconds(animatorInfo.ReloadStartTime);
            animatorInfo.SetReloadSpeed(reloadLateSecond);
            var runningWait = new WaitForSeconds(reloadLateSecond);
            for (int i = 0; i < repeatCount; ++i)
            {
                yield return runningWait;
                base.ReLoadBullet(1);
            }
            yield return runningWait;
            animatorInfo.PlayReloadEnd();
        }

        #endregion

    }
}