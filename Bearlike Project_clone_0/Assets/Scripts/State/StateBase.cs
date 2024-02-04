using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;
using UnityEngine.Serialization;

namespace State.StateClass.Base
{    
    /// <summary>
    /// Object의 상태를 나타내는 열거형
    /// </summary>
    public enum ObjectProperty
    {
        Normality = 0b_0000_0000,           // 정상
        Poisoned = 0b_0000_0001,            // 중독
        Weak = 0b_0000_0010,                // 취약 => 최종 데미지 1.5배 증가
    }
    
    /// <summary>
    /// 기본 능력치를 나타내는 Class
    /// </summary>
    public abstract class StateBase : NetworkBehaviour
    {
        // Member Variable
        public StatusValue<int> hp = new StatusValue<int>();                              // 체력        
        public StatusValue<int> attack = new StatusValue<int>();               // 공격력
        public StatusValue<int> defence = new StatusValue<int>();              // 방어력
        public StatusValue<float> avoid = new StatusValue<float>();           // 회피
        public StatusValue<int> moveSpeed = new StatusValue<int>();          // 이동 속도
        public StatusValue<float> attackSpeed = new StatusValue<float>();     // 공격 속도

        public StatusValue<int> force = new StatusValue<int>();               // 힘
        public int condition;                                               // 상태

        public int property = 0;

        #region Variable Paramiter
        public bool IsDie => hp.isMin;

        #endregion
        
        // Member Function
        public abstract void Initialization();
        public abstract void MainLoop();
        
        public abstract bool ApplyDamage(float damage, ObjectProperty property); // MonsterRef instigator,
        // public abstract void BePoisoned();

        // DeBug Function
        public abstract void ShowInfo();

        #region Condition Interface Functon

        // ICondition Interface Function
        public abstract bool On(ObjectProperty condition);
            
        public abstract bool NormalityIsOn();
        public abstract bool PoisonedIsOn();
        public abstract bool WeakIsOn();
            
        public abstract void AddCondition(ObjectProperty condition);
        public abstract void DelCondition(ObjectProperty condition);
        
        #endregion

    }
}