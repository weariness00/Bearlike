using System.Collections.Generic;
using Fusion;
using Skill.Container;
using State.StateClass;
using UnityEngine;

namespace Skill
{
    public class SkillSystem : NetworkBehaviour
    {
        public readonly List<SkillBase> SkillList = new List<SkillBase>();

        private PlayerStatus _playerStatus;
        
        private void Start()
        {
            _playerStatus = gameObject.GetComponent<PlayerStatus>();
            
            // HACK : 테스트용
            SkillList.Add(new FlippingCoin(_playerStatus));
        }

        public override void FixedUpdateNetwork()
        {
            foreach (var skill in SkillList)
            {
                skill.MainLoop();
            }

            var ps = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            ps.ShowInfo();
            ps = GameObject.Find("Remote Player").GetComponent<PlayerStatus>();
            ps.ShowInfo();
        }
    }
}