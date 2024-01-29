using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;

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
    public abstract class State : NetworkBehaviour
    {
        // Member Variable
        public StatusValue<int> Hp { get; set; }                               // 체력        
        public StatusValue<int> Attack { get; set; }                // 공격력
        public StatusValue<int> Defence { get; set; }               // 방어력
        public StatusValue<float> Avoid { get; set; }             // 회피
        public StatusValue<int> MoveSpeed { get; set; }             // 이동 속도
        public StatusValue<float> AttackSpeed { get; set; }       // 공격 속도
        
        public StatusValue<int> Force { get; set; }                 // 힘
        public int Condition { get; set; }                                                   // 상태

        
        // Member Function
        public abstract void Initialization();
        public abstract void MainLoop();
        
        public abstract bool ApplyDamage(float damage, ObjectProperty property); // MonsterRef instigator,
        // public abstract void BePoisoned();

        // DeBug Function
        public abstract void ShowInfo();
        
        // ICondition Interface Function
        public abstract bool On(ObjectProperty condition);
            
        public abstract bool NormalityIsOn();
        public abstract bool PoisonedIsOn();
        public abstract bool WeakIsOn();
            
        public abstract void AddCondition(ObjectProperty condition);
        public abstract void DelCondition(ObjectProperty condition);
    }
}