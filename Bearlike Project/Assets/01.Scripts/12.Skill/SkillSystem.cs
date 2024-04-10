using System.Collections.Generic;
using System.Linq;
using Manager;
using Photon;
using UnityEngine;

namespace Skill
{
    public class SkillSystem : MonoBehaviour
    {
        public List<SkillBase> skillList = new List<SkillBase>();

        private void Start()
        {
            skillList = GetComponentsInChildren<SkillBase>().ToList();
        }

        private void Update()
        {
            foreach (var skill in skillList)
            {
                if (skill.isInvoke == false)
                {
                    skill.MainLoop();
                }
            }
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