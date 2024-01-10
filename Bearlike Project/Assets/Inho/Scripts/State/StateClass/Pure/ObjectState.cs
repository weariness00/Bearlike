//---------------------------------------------------------------------------------
// Object의 기본 능력치를 나타내는 Class
//---------------------------------------------------------------------------------

namespace Inho.Scripts.State
{
    public abstract class ObjectState
    {
        // Member Variable
        protected float mHP;                // 체력        
        protected float mAtk;            // 공격력
        protected float mDfs;           // 방어력
        protected float mAvoid;             // 회피
        protected float mspeed;             // 이동 속도
        
        protected float mForce;             // 힘
        protected eCondition mCondition;    // 상태

        
        // Member Function
        public abstract void Initialization();
        
        public abstract void HealingHP(); // type별로 다른 비율
        public abstract void BeDamaged(float attack);
        
        // DeBug Function
        public abstract void ShowInfo();
    }
}