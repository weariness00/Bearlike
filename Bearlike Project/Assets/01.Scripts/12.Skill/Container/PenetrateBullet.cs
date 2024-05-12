using Player;
using UnityEngine;
using Weapon.Bullet;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 총알에 관통을 부여해주는 스킬
    /// </summary>
    public class PenetrateBullet : SkillBase
    {
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.weaponSystem.equipment.IsGun)
                {
                    var gun = pc.weaponSystem.equipment as GunBase;
                    gun.BeforeShootAction += ReinforceBullet;
                }
            }
        }

        public override void MainLoop()
        {
        }

        public override void Run()
        {
        }

        /// <summary>
        /// 총알이 관통이 되도록 하는 함수
        /// </summary>
        public void ReinforceBullet(BulletBase bullet)
        {
            bullet.penetrateCount = level;
        }
    }
}

