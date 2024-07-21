using Player;
using UnityEngine;
using Util;
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
                    gun.penetrateCount = level;
                }
            }
        }

        public override void MainLoop(){}
        public override void Run(){}

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Level)"))
                explain = explain.Replace("(Level)", $"{level.Current}");
            
            explain = StringExtension.CalculateNumber(explain);
        }
    }
}

