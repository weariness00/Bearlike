using System.Collections.Generic;
using System.Linq;
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

        public void AddSkill(SkillBase skill)
        {
            skillList.Add(skill);
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
    }
}