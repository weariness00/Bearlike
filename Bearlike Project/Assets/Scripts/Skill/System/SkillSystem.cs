using System;
using Inho.Scripts.Skill.SkillClass.FirstDoll;
using UnityEngine;

namespace Inho.Scripts.Skill.System
{
    public class SkillSystem : MonoBehaviour
    {
        private FirstDoll IndividualSkill;
        
        private void Start()
        {
             IndividualSkill = new FirstDoll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                IndividualSkill._filppingCoin.Run();
            }
        }
    }
}