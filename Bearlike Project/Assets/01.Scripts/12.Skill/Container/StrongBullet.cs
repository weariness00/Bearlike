using Player;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    public class StrongBullet : SkillBase
    {
        public override void Earn(GameObject earnTargetObject)
        {
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.weaponSystem.equipment is GunBase gun)
                {
                    gun.status.additionalStatusList.Add(status);
                }
            }
        }

        public override void MainLoop()
        {
        }

        public override void Run(GameObject runObject)
        {
        }

        public override void LevelUp()
        {
            status.damageMagnification = 1f + 0.3f * level.Current;
        }
    }
}

