using System.Collections.Generic;
using Inho.Scripts.State.StateClass.Pure;
using Script.GameStatus;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Inho.Scripts.State.StateClass
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public class PlayerState : Pure.State
    {
        // Member Variable
        private StatusValue<int> mLevel = new StatusValue<int>();               // 레벨
        private StatusValue<int> mExp = new StatusValue<int>();                 // 경험치
        private List<int> mExpAmount = new List<int>();                         // 레벨별 경험치량
        
        // Member Function
        // ObjectState abstract class Function
        public PlayerState()
        {
            mHP.max = 100;
            mHP.min = 0;
            mHP.current = 100;

            mAtk.max = 100;
            mAtk.min = 1;
            mAtk.current = 10;

            mDfs.max = 100;
            mDfs.min = 1;
            mDfs.current = 1;

            mAvoid.max = 100.0f;
            mAvoid.min = 0.0f;
            mAvoid.current = 0.0f;
            
            mspeed.max = 100;
            mspeed.min = 1;
            mspeed.current = 1;

            mAtkSpeed.max = 10.0f;
            mAtkSpeed.min = 0.5f;
            mAtkSpeed.current = 1.0f;

            mForce.max = 1000;
            mForce.min = 0;
            mForce.current = 10;
            
            mCondition = (int)eCondition.Normality;
            
            for(int i = 0; i < 10; ++i)
                mExpAmount.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용
            
            mLevel.max = 10;
            mLevel.min = 1;
            mLevel.current = 1;

            mExp.max = mExpAmount[mLevel.current];
            mExp.min = 0;
            mExp.current = 0;

            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }
        
        
        // Loop
        public override void MainLoop()
        {
            if (PoisonedIsOn())
            {
                BePoisoned(Constants.POISONDAMAGE);
                ShowInfo();
            }
        }
        
        public override void Initialization()
        {
            // 혹시 모를 함수
        }
        // Loop
        

        // HP
        // // 스킬, 무기, 캐릭터 스텟을 모두 고려한 함수 구현 필요
        public void BePoisoned(int value)
        {
            mHP.current -= value;
        }
        
        public override void BeDamaged(float attack)
        {   
            if ((Random.Range(0.0f, 99.9f) < mAvoid.current)) return;
            
            var damageRate = math.log10((attack / mDfs.current) * 10);

            if (WeakIsOn()) damageRate *= 1.5f;

            mHP.current -= (int)(damageRate * attack);
        }
        // HP
        
        // LV
        public void IncreaseExp(int value)
        {
            mExp.current += value;

            while (mExpAmount[mLevel.current] <= mExp.current && mLevel.max > mLevel.current)
            {
                mExp.current -= mExpAmount[mLevel.current];
                mLevel.current++;
                mExp.max = mExpAmount[mLevel.current];
                if(mLevel.max <= mLevel.current) Debug.Log("최대 레벨 도달");
            }
        }
        // LV

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  mHP.current + $" 공격력 : " + mAtk.current + $" 공격 속도 : " + mAtkSpeed.current + $" 상태 : " + (eCondition)mCondition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }
        
        
        // ICondition Interface Function
        public override bool On(int condition) { return (mCondition & condition) == condition; }

        public override bool NormalityIsOn() { return On((int)eCondition.Normality); }
        public override bool PoisonedIsOn() { return On((int)eCondition.Poisoned); }
        public override bool WeakIsOn() { return On((int)eCondition.Weak); }
        
        public override void AddCondition(int condition)
        {
            if(!On(condition)) mCondition |= condition;
        }
        
        public override void DelCondition(int condition)
        {
            if(On(condition)) mCondition ^= condition;
        }
    }
}