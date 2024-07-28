using System.Collections.Generic;
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
            base.FixedUpdateNetwork();
            
            if (!GameManager.Instance.isControl)
                return;

            if (GetInput(out PlayerInputData data))
            {
                if (!IsCursor)
                {
                    SkillControl(data);
                }
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

                uiController.skillCanvas.gameObject.SetActive(true);

                uiController.skillCanvas.SetFirstSkill(FlippingCoin);
                uiController.skillCanvas.SetSecondSkill(tmpSkill);
                uiController.skillCanvas.SetUltimateSkill(ultimateSkill);
                
                uiController.skillCanvas.Initialize();
            }

            FlippingCoin.LevelUp();
            tmpSkill.LevelUp();
            ultimateSkill.LevelUp();
        }

        void SkillControl(PlayerInputData data)
        {
            if (HasInputAuthority == false)
                return;
            if (status.isInjury || status.isRevive)
                return;
            
            if (data.FirstSkill)
            {
                uiController.skillCanvas.StartCoolTime(FlippingCoin);
                FlippingCoin.RunRPC();
            }
            else if (data.SecondSkill)
            {
                uiController.skillCanvas.StartCoolTime(tmpSkill);
                tmpSkill.RunRPC();
            }
            else if (data.Ultimate)
            {
                uiController.skillCanvas.StartCoolTime(ultimateSkill);
                ultimateSkill.RunRPC();
            }
        }
    }
}