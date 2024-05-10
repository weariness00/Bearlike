using GamePlay;
using Photon;
using Skill;
using Skill.Container;
using UI.Skill;
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
            if(!GameManager.Instance.isControl)
                return;
            
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
            tmpSkill = skillSystem.GetSkillFromName("SniperContinuousMode");
            ultimateSkill = skillSystem.GetSkillFromName("Clean Shoot");
            
            if (HasInputAuthority)
            {
                FlippingCoin.Object.AssignInputAuthority(Object.InputAuthority);
                tmpSkill.Object.AssignInputAuthority(Object.InputAuthority);
                ultimateSkill.Object.AssignInputAuthority(Object.InputAuthority);
                
                skillCanvas.gameObject.SetActive(true);
                
                skillCanvas.SetFirstSkill(FlippingCoin);
                skillCanvas.SetSecondSkill(tmpSkill);
                skillCanvas.SetUltimateSkill(ultimateSkill);

                skillCanvas.Initialize();
            }
            
            FlippingCoin.LevelUp();
            tmpSkill.LevelUp();
            ultimateSkill.LevelUp();
        }

        void SkillControl(PlayerInputData data)
        {
            if (HasInputAuthority == false || status.isInjury)
            {
                return;
            }
            
            if (data.FirstSkill)
            {
                FlippingCoin.Run(gameObject);
                skillCanvas.StartCoolTime(FlippingCoin);
            }
            else if (data.SecondSkill)
            {
                tmpSkill.Run(gameObject);
                skillCanvas.StartCoolTime(tmpSkill);
            }
            else if (data.Ultimate)
            {
                ultimateSkill.Run(gameObject);
                skillCanvas.StartCoolTime(ultimateSkill);
            }
        }
    }
}