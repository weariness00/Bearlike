﻿using State.StateSystem;
using UnityEngine;

namespace Skill.SkillClass.SecondDoll
{
    /// <summary>
    /// 회피 시스템 작동 : 스킬을 사용하면 10초간 회피률이 1.3배 상승한다.
    ///                  지속 시간 : 10초 / 재사용 대기 시간 : 30초
    /// </summary>
    public class AvoidingSystemOperation : Inho.Scripts.Skill.SkillClass.Base.Skill
    {
        public AvoidingSystemOperation()
        {
            
        }

        public override void Run()
        {
            var playerState = GameObject.Find("Player").GetComponent<StateSystem>().GetState();

            playerState.avoid.Current *= 1.3f;
        }
    }
}