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
        [Header("Skinned Mesh")]
        [SerializeField] private GameObject armMehs;
        [SerializeField] private List<GameObject> otherMeshes;
        
        private SkillBase FlippingCoin;
        private SkillBase tmpSkill;
        private SkillBase ultimateSkill;

        public override void Spawned()
        {
            base.Spawned();
            SkillInit();

            if (HasInputAuthority)
            {
                armMehs.layer = LayerMask.NameToLayer("Weapon");
                foreach (var otherMesh in otherMeshes)
                    otherMesh.layer = LayerMask.NameToLayer("Volum");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!GameManager.Instance.isControl)
                return;

            if (status.isInjury || status.isRevive)
                return;

            if (GetInput(out PlayerInputData data))
            {
                if (!data.Cursor)
                {
                    SkillControl(data);
                }
            }

            base.FixedUpdateNetwork();
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
            if (HasInputAuthority == false)
                return;

            if (data.FirstSkill)
            {
                skillCanvas.StartCoolTime(FlippingCoin);
                FlippingCoin.RunRPC();
            }
            else if (data.SecondSkill)
            {
                skillCanvas.StartCoolTime(tmpSkill);
                tmpSkill.RunRPC();
            }
            else if (data.Ultimate)
            {
                skillCanvas.StartCoolTime(ultimateSkill);
                ultimateSkill.RunRPC();
            }
        }
    }
}