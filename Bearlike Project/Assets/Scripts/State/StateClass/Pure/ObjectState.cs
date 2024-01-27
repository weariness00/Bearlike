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
        public StatusValue<int> _hp = new StatusValue<int>();                    // 체력        
        public StatusValue<int> _attack = new StatusValue<int>();                   // 공격력
        public StatusValue<int> _defence = new StatusValue<int>();                   // 방어력
        public StatusValue<float> _avoid = new StatusValue<float>();             // 회피
        public StatusValue<int> _moveSpeed = new StatusValue<int>();                 // 이동 속도
        public StatusValue<float> _attackSpeed = new StatusValue<float>();          // 공격 속도
        
        public StatusValue<int> _force = new StatusValue<int>();                 // 힘
        public int _condition;    // 상태

        
        // Member Function
        public abstract void Initialization();
        public abstract void MainLoop();
        
        public void HealingHP(int value) { _hp.Current += value; }
        public abstract void BeDamaged(float damage);
        // public abstract void BePoisoned();

        // DeBug Function
        public abstract void ShowInfo();
    }
}