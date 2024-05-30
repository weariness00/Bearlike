using Player;
using Status;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    public class StrongBullet : SkillBase
    {
        private float _damageMultiplePerLevel = 0f; // 레벨당 대미지 배율

        #region Unity Event Function

        public override void Start()
        {
            var statusData = GetStatusData(id);
            _damageMultiplePerLevel = statusData.GetFloat("Damage Multiple Per Level");
        }

        #endregion
        
        
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.weaponSystem.equipment is GunBase gun)
                {
                    gun.status.AddAdditionalStatus(status);
                }
            }
        }

        public override void MainLoop()
        {
        }

        public override void Run()
        {
        }

        public override void LevelUp()
        {
            base.LevelUp();
            
            status.criticalHitMultiple = 1f + _damageMultiplePerLevel * level.Current;
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Level)"))
                explain = explain.Replace("(Level)", $"{level.Current}");
        }
    }
}

