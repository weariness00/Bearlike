//---------------------------------------------------------------------------------
// Object의 기본 능력치를 나타내는 Class
//---------------------------------------------------------------------------------

namespace Inho.Scripts.State
{
    public abstract class ObjectState
    {
        // Member Variable
        protected float mHP;                // 체력        
        protected float mForce;             // 힘
        protected eCondition mCondition;    // 상태

        
        // Member Function
        public abstract void Initialization();

        public float GetHP() { return mHP; }
        public void SetHp(float input) { mHP = input; }
        public abstract void HealingHP(); // type별로 다른 비율
        public abstract void BeDamaged(float Damage);
        
        // DeBug Function
        public abstract void ShowInfo();
    }
}