using System.Collections.Generic;
using System.Linq;
using Fusion;
using Manager;
using Photon;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 객체가 가지고 있는 스킬들을 관리해주는 시스템
    /// </summary>
    public class SkillSystem : NetworkBehaviourEx
    {
        public List<SkillBase> skillList = new List<SkillBase>();
        public int SkillLength => skillList.Count;

        [SerializeField] private float coolTimeReductionRate; // 모든 스킬들의 쿨타임 감소율

        #region Unity Event Function
        
        private void Start()
        {
            skillList = GetComponentsInChildren<SkillBase>().ToList();
        }

        public override void FixedUpdateNetwork()
        {
            foreach (var skill in skillList)
            {
                if (skill.isInvoke)
                {
                    skill.MainLoop();
                }
            }
        }

        #endregion

        public void AddSkill(SkillBase skill)
        {
            skillList.Add(skill);
            skill.SetCoolTimeReductionRate(coolTimeReductionRate);
        }

        public SkillBase GetSkillFromId(int id)
        {
            foreach (var skillBase in skillList)
            {
                if (skillBase.id.Equals(id))
                {
                    return skillBase;
                }
            }
            
            DebugManager.LogError($"해당 ID[{id}]의 스킬이 존재하지 않습니다.");

            return null;
        }

        public bool TryGetSkillFromID(int id, out SkillBase skill)
        {
            foreach (var skillBase in skillList)
            {
                if (skillBase.id.Equals(id))
                {
                    skill = skillBase;
                    return true;
                }
            }

            skill = null;
            return false;
        }

        public SkillBase GetSkillFromName(string skillName)
        {
            foreach (var skillBase in skillList)
            {
                if (skillBase.skillName.Equals(skillName))
                {
                    return skillBase;
                }
            }
            
            DebugManager.LogError($"[{skillName}]이라는 스킬이 존재하지 않습니다.");

            return null;
        }

        public List<SkillBase> GetActiveSkills()
        {
            List<SkillBase> activeSkillList = new List<SkillBase>();
            foreach (var skill in skillList)
            {
                if (skill.type == SKillType.Active && skill.level.Current > 0)
                {
                    activeSkillList.Add(skill);
                }
            }

            return activeSkillList;
        }

        public void SetCoolTimeReductionRate(float rate)
        {
            var activeSkillList = GetActiveSkills();

            foreach (var skill in activeSkillList)
            {
                var realRate = skill.GetCoolTimeReductionRate() - coolTimeReductionRate + rate;
                skill.SetCoolTimeReductionRate(realRate);
            }

            coolTimeReductionRate = rate;
        }
    }
}