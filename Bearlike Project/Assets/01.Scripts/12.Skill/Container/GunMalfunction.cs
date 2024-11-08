﻿using Manager;
using Player;
using UnityEngine;
using Weapon.Bullet;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 총기 오작동
    /// 1발을 쏘면 1발이 더 나간다 추가적인 탄환 소모는 없다.
    /// </summary>
    public class GunMalfunction : SkillBase
    {
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.weaponSystem.equipment is GunBase gun)
                {
                    gun.AfterFireAction += () =>
                    {
                        AdditionalBullet(gun);
                        DebugManager.Log($"{name}으로 인해 추가적인 탄환 발사");
                    };
                }
            }
        }   

        public override void MainLoop()
        {
        }

        public override void Run()
        {
        }
        
        private void AdditionalBullet(GunBase gun)
        {
            if (HasStateAuthority && gun.magazine.Current > 0)
            {
                var dst = gun.CheckRay();
                var nuckBack = gun.status.GetAllNuckBack();
                
                Runner.SpawnAsync(gun.bullet.gameObject, gun.fireTransform.position - gun.fireTransform.forward, gun.fireTransform.rotation, null,
                    (runner, o) =>
                    {
                        var b = o.GetComponent<BulletBase>();

                        b.OwnerId = gun.OwnerId;
                        b.OwnerGunId = gun.Object.Id;
                        b.KnockBack = nuckBack;;
                        b.destination = gun.fireTransform.position + (dst * gun.status.attackRange);
                        b.PenetrateCount = gun.penetrateCount;
                    });
                
            }
        }
    }
}

