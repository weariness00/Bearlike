using System.Collections.Generic;
using Fusion;
using Manager;
using UnityEngine;

namespace Skill
{
    public class SkillSystem : MonoBehaviour
    {
        public List<SkillBase> skillList = new List<SkillBase>();
        private Dictionary<string, SkillBase> _skillDictionary = new Dictionary<string, SkillBase>();

        // private PlayerStatus _playerStatus;

        private void Start()
        {
            // _playerStatus = gameObject.GetComponent<PlayerStatus>();
            //
            // // HACK : 테스트용
            // skillList.Add(new FlippingCoin(_playerStatus));
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

            // var ps = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            // ps.ShowInfo();
            // ps = GameObject.Find("Remote Player").GetComponent<PlayerStatus>();
            // ps.ShowInfo();
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