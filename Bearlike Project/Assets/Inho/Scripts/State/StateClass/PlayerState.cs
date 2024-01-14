using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Inho.Scripts.State
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public class PlayerState : State
    {
        // Member Variable
        private int mPlayerID;          // 아이디
        private PlayerJob mPlayerJob;   // 직업

        private int mLevel;             // 레벨
        private int mExp;               // 경험치
        private List<int> mExpAmount = new List<int>();   // 레벨별 경험치량
        
        // Member Function
        // ObjectState abstract class Function
        public override void Initialization()
        {
            mLevel = 1;
            mExp = 0;
            
            mHP = 100.0f;
            mAtk = 1.0f;
            mDfs = 1.0f;
            mAvoid = 0.0f;
            mspeed = 1.0f;
            
            mForce = 10.0f;
            mCondition = (int)eCondition.Normality;
            
            for(int i = 0; i < 10; ++i)
                mExpAmount.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용

            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }
        
        // HP
        // // 스킬, 무기, 캐릭터 스텟을 모두 고려한 함수 구현 필요
        public override void BeDamaged(float attack)
        {   
            if ((Random.Range(0.0f, 99.9f) < mAvoid)) return;
            
            var damageRate = math.log10((attack / mDfs) * 10);

            if (WeakIsOn()) damageRate *= 1.5f;

            mHP -= damageRate * attack;
        }
        // HP
        
        // LV
        public void IncreaseExp(int value)
        {
            mExp += value;

            if (mExpAmount[mLevel] <= mExp)
            {                
                mExp -= mExpAmount[mLevel];
                mLevel++;
            }
        }
        // LV

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  mHP + $" 힘 : " + mForce + $" 상태 : " + (eCondition)mCondition);
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