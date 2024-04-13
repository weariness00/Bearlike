using Photon;
using Skill;
using Skill.Container;
using UnityEngine;

namespace Player.Container
{
    public class FirstBear : PlayerController
    {
        private SkillBase FlippingCoin;
        private SkillBase tmpSkill;
        private SkillBase ultimateSkill;

        public override void Spawned()
        {
            base.Spawned();
            SkillInit();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if(status.isInjury || status.isRevive)
                return;
            
            if (GetInput(out PlayerInputData data))
            {
                if (data.Cursor)
                    return;

                SkillControl(data);
            }
        }

        void SkillInit()
        {
            FlippingCoin = skillSystem.GetSkillFromName("FlippingCoin");
            tmpSkill = skillSystem.GetSkillFromName("SniperContinousMode");
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
                FlippingCoin.Run(gameObject);
            }
            else if (data.SecondSkill)
            {
                tmpSkill.Run(gameObject);
            }
            else if (data.Ultimate)
            {
                ultimateSkill.Run(gameObject);
            }
        }
    }
}