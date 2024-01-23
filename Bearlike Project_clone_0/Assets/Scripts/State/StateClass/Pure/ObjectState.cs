using Scripts.State.GameStatus;
using UnityEngine;

namespace State.StateClass.Pure
{
    /// <summary>
    /// // Object의 기본 능력치를 나타내는 Class
    /// </summary>
    public abstract class ObjectState : MonoBehaviour
    {
        // Member Variable
        protected StatusValue<int> mHP = new StatusValue<int>();                    // 체력        
        protected StatusValue<int> mAtk = new StatusValue<int>();                   // 공격력
        protected StatusValue<int> mDfs = new StatusValue<int>();                   // 방어력
        protected StatusValue<float> mAvoid = new StatusValue<float>();             // 회피
        protected StatusValue<int> mspeed = new StatusValue<int>();                 // 이동 속도
        protected StatusValue<float> mAtkSpeed = new StatusValue<float>();          // 공격 속도
        
        protected StatusValue<int> mForce = new StatusValue<int>();                 // 힘
        protected int mCondition;    // 상태

        
        // Member Function
        public abstract void Initialization();
        public abstract void MainLoop();
        
        public void HealingHP(int value) { mHP.Current += value; }
        public abstract void BeDamaged(float attack);
        // public abstract void BePoisoned();

        public void SetAtk(int value) { mAtk.Current = value; }
        public int GetAtk() { return mAtk.Current; }
        
        public void SetAtkSpeed(float value) { mAtkSpeed.Current = value; }
        public float GetAtkSpeed() { return mAtkSpeed.Current; }

        // DeBug Function
        public abstract void ShowInfo();
    }
}