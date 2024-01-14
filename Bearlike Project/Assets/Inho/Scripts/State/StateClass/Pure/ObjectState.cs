namespace Inho.Scripts.State
{
    /// <summary>
    /// // Object의 기본 능력치를 나타내는 Class
    /// </summary>
    public abstract class ObjectState
    {
        // Member Variable
        protected float mHP;                // 체력        
        protected float mAtk;               // 공격력
        protected float mDfs;               // 방어력
        protected float mAvoid;             // 회피
        protected float mspeed;             // 이동 속도
        
        protected float mForce;             // 힘
        protected int mCondition;    // 상태

        
        // Member Function
        public abstract void Initialization();
        
        public void HealingHP(float value) { mHP += value; }
        public abstract void BeDamaged(float attack);

        public float GetAtk() { return mAtk; }

        // DeBug Function
        public abstract void ShowInfo();
    }
}