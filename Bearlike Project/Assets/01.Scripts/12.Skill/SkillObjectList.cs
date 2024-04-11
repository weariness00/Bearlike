using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Util;

namespace Skill
{
    public class SkillObjectList : Singleton<SkillObjectList>
    {
        #region Static

        public static int SkillCount => Instance.skillList.Count;
        
        public static SkillBase GetFromIndex(int index)
        {
            if (index >= Instance.skillList.Count)
            {
                DebugManager.LogError($"SKill List의 최대 길이는 [{Instance.skillList.Count}]입니다.");
                return null;
            }

            return Instance.skillList[index];
        }
        
        public static SkillBase GetFromID(int id)
        {
            foreach (var skillBase in Instance.skillList)
            {
                if (skillBase.id == id)
                {
                    return skillBase;
                }
            }
            
            DebugManager.LogError($"ID : {id} 인 스킬이 존재하지 않습니다.");
            
            return null;
        }

        public static SkillBase GetFromName(string skillName)
        {
            foreach (var skillBase in Instance.skillList)
            {
                if (skillBase.skillName == skillName)
                {
                    return skillBase;
                }
            }
            
            DebugManager.LogError($"Name : {skillName} 인 스킬이 존재하지 않습니다.");
            
            return null;
        }
        
        #endregion

        public List<SkillBase> skillList = new List<SkillBase>();

        #region Unity Event Function

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        #endregion
    }
}

