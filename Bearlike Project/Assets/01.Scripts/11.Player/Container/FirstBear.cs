using Photon;
using Skill;
using Skill.Container;
using UnityEngine;

namespace Player.Container
{
    public class FirstBear : PlayerController
    {
        private SkillBase ultimateSkill;

        public override void Spawned()
        {
            base.Spawned();
            SkillInit();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if (GetInput(out PlayerInputData data))
            {
                if (data.Cursor)
                    return;

                SkillControl(data);
            }
        }

        void SkillInit()
        {
            ultimateSkill = skillSystem.GetSkillFromName("Clean Shoot");
        }

        void SkillControl(PlayerInputData data)
        {
            if (HasInputAuthority == false)
            {
                return;
            }
            
            if (data.FirstSkill)
            {
            }
            else if (data.Ultimate)
            {
                ultimateSkill.Run(gameObject);
            }
        }
    }
}