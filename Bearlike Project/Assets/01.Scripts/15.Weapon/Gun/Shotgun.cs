using System.Collections;
using Fusion;
using Status;
using UnityEngine;
using Weapon.Bullet;

namespace Weapon.Gun
{
    public class Shotgun : GunBase
    {
        [SerializeField] private float reloadSpeed = 0.5f; // 재장전 속도

        [SerializeField] private float bulletRadian; // 산탄 정도(원의 반지름)
        public bool bknock;

        private Coroutine _reloadCoroutine;

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
            // json화
            ammo.Max = 36;
            ammo.Current = ammo.Max;
        }

        public override void Shoot(bool isDst = true)
        {
            // 다른 클라가 했으면 자기 말고 다른 클라의 코드를 실행해야지
            if (FireLateTimer.Expired(Runner))
            {
                FireLateTimer = TickTimer.CreateFromSeconds(Runner, fireLateSecond);
                if (magazine.Current != 0)
                {
                    var dst = CheckRay();

                    if (shootEffect != null) shootEffect.Play();
                    // bullet.hitEffect = hitEffect;
                    // bullet.bknock = bknock;
                    // bullet.maxMoveDistance = status.attackRange;

                    if (HasStateAuthority)
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            Runner.SpawnAsync(bullet.gameObject, transform.position + dst, transform.rotation, null,
                                (runner, o) =>
                                {
                                    Vector3 randomVector3 = new Vector3(Random.Range(-bulletRadian, bulletRadian),
                                        Random.Range(-bulletRadian, bulletRadian), Random.Range(-bulletRadian, bulletRadian));
                                    
                                    var b = o.GetComponent<BulletBase>();
                                    b.status.AddAdditionalStatus(status);
                                    b.OwnerId = OwnerId;
                                    b.hitEffect = hitEffect;
                                    b.bknock = bknock;
                                    b.status.attackRange.Max = status.attackRange.Max;
                                    b.status.attackRange.Current = status.attackRange.Current;
                                    b.destination = fireTransform.position + (dst * status.attackRange) + randomVector3;
                                });
                        }
                    }

                    SetMagazineRPC(StatusValueType.Current, --magazine.Current);
                    SoundManager.Play(shootSound);
                }
                else
                {
                    SoundManager.Play(emptyAmmoSound);
                }
            }

            if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
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

                if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = StartCoroutine(ReloadCoroutine(reloadSpeed, needChargingAmmoCount));
            }
        }

        IEnumerator ReloadCoroutine(float waitTime, int repeatCount)
        {
            for (int i = 0; i < repeatCount; ++i)
            {
                yield return new WaitForSeconds(waitTime);
                base.ReLoadBullet(1);
            }
        }

        #endregion
    }
}