using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Inho.Scripts.State
{
    public class PlayerState : State
    {
        // Member Variable
        private int mPlayerID;          // 아이디
        private PlayerJob mPlayerJob;   // 직업

        private int mLevel;             // 레벨
        private int mExp;               // 경험치
        
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
            mCondition = eCondition.Normality;

            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }
        
        // HP
        public override void HealingHP() { mHP += 10.0f; }

        public override void BeDamaged(float attack)
        {
            if ((Random.Range(0.0f, 100.0f) < mAvoid)) return;
            
            var DamageRate = math.log10((attack / mDfs) * 10);

            if (WeakIsOn()) DamageRate *= 1.5f;

            mHP -= DamageRate * attack;
        }
        // HP
        
        // LV
        // public void IncreaseExp(int value)
        // {
        //     mExp += value;
        //     
        // }
        //

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log("체력 : " +  mHP + " 힘 : " + mForce + " 상태 : " + mCondition);
        }
        
        
        // ICondition Interface Function
        public override bool On(eCondition condition) { return (mCondition & condition) == condition; }

        public override bool NormalityIsOn() { return On(eCondition.Normality); }
        public override bool PoisonedIsOn() { return On(eCondition.Poisoned); }
        public override bool WeakIsOn() { return On(eCondition.Weak); }
        
        public override void AddCondition(eCondition condition)
        {
            mCondition |= condition;
        }
        
        public override void DelCondition(eCondition condition)
        {
            mCondition ^= condition;
        }
    }
}