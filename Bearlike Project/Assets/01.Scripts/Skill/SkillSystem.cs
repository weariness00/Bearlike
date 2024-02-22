using System.Collections.Generic;
using Fusion;
using Skill.Container;
using UnityEngine;

namespace Skill
{
    public class SkillSystem : NetworkBehaviour
    {
        public List<SkillBase> skillList = new List<SkillBase>(); 
        
        private void Start()
        {
            // HACK : 테스트용
            skillList.Add(new FlippingCoin());
        }

        public override void FixedUpdateNetwork()
        {
            skillList[0].MainLoop();
        }
    }
}