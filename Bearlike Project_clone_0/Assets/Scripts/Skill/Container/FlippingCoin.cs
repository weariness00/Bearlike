using State.StateSystem;
using UnityEngine;

namespace Skill.SkillClass.FirstDoll.UniqueSkill
{
    /// <summary>
    /// 동전 던지기(첫째곰) : 앞 면이 나올시에는 공속이 1.5배 상승, 뒷 면이 나올시에는 데미지 1.2배 상승
    ///                    지속 시간은 10초 정도로 설정 AND 쿨타임은 30초로 설정
    /// </summary>
    public class FlippingCoin : Inho.Scripts.Skill.SkillClass.Base.Skill
    {
        public FlippingCoin()
        {
            // mDuration.max = mDuration.current = 10.0f;
            // mDuration.min = 0.0f;
            //
            // mCoolTime.max = mCoolTime.current = 30.0f;
            // mCoolTime.min = 0.0f;
        }
        
        public override void Run()
        {
            var playerState = GameObject.Find("Player").GetComponent<StateSystem>().State;
            
            if (Random.Range(0, 2) == 0)
            {
                // HEAD
                playerState.attackSpeed.Current *= 1.5f;
            }
            else
            {
                // TAIL
                playerState.attack.Current = (int)(playerState.attack.Current * 1.2f);
            }
        }
    }
}