using System.Collections.Generic;
using Fusion;
using Skill.Container;
using State.StateClass;
using UnityEngine;

namespace Skill
{
    public class SkillSystem : NetworkBehaviour
    {
        public List<SkillBase> skillList = new List<SkillBase>(); 
        
        private void Start()
        {
            // HACK : 테스트용
            skillList.Add(new FlippingCoin(gameObject.GetComponent<PlayerStatus>()));
        }

        public override void FixedUpdateNetwork()
        {
            foreach (var skill in skillList)
            {
                skill.MainLoop();
            }

            var ps = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            ps.ShowInfo();
            // ps = GameObject.Find("Remote Player").GetComponent<PlayerStatus>();
            // ps.ShowInfo();
        }
    }
}