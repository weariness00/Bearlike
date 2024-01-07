namespace Inho.Scripts
{
    public abstract class State
    {
        // Member Variable
        protected float mHP;                // 체력        
        protected float mForce;              // 힘
        protected eCondition mCondition;    // 상태
        
        // Member Function
        public abstract void Initialization();

        public void SetHp(float input) { mHP = input; }
        public abstract void HealingHP(); // type별로 다른 비율
    }
}