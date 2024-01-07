using UnityEngine;

namespace Inho.Scripts.State
{
    public class PlayerState : State
    {
        // Member Variable
        private int mPlayerID;
        private PlayerJob mPlayerJob;
        
        // Member Function
        // ObjectState abstract class Function
        public override void Initialization()
        {
            mHP = 100;
            mForce = 10.0f;
            mCondition = eCondition.Normality;

            // mPlayerID 초기화 필요
            // mPlayerJob 초기화 필요
        }
        
        public override void HealingHP() { mHP += 10.0f; }

        public override void BeDamaged(float Damage)
        {
            var DamageRate = 1.0f;

            if (WeakIsOn()) DamageRate *= 1.5f;

            mHP -= DamageRate * Damage;
        }
        
        
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