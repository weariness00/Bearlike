using Fusion;
using Status;
using UnityEngine;
using UnityEngine.Serialization;

namespace State.StateClass.Base
{    
    /// <summary>
    /// Object의 상태를 나타내는 열거형
    /// </summary>
    public enum CrowdControl
    {
        Normality = 0b_0000_0000,           // 정상
        Poisoned = 0b_0000_0001,            // 중독
        Weak = 0b_0000_0010,                // 취약 => 최종 데미지 1.5배 증가
    }
    
    /// <summary>
    /// 기본 능력치를 나타내는 Class
    /// </summary>
    public abstract class StatusBase : NetworkBehaviour
    {
        #region Member Variable
        
        public StatusValue<int> hp = new StatusValue<int>();                  // 체력        
        public StatusValue<int> attack = new StatusValue<int>();              // 공격력
        public StatusValue<int> defence = new StatusValue<int>();             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>();           // 회피
        public StatusValue<int> moveSpeed = new StatusValue<int>();           // 이동 속도
        public StatusValue<float> attackSpeed = new StatusValue<float>();     // 공격 속도
        public StatusValue<float> attackRange = new StatusValue<float>();
        
        public StatusValue<int> force = new StatusValue<int>();               // 힘
        public int condition;                                                 // 상태
        public int property;                                                  // 속성

        #region 프로퍼티
        
        // [Networked]
        // public int Hp
        // {
        //     get => hp.Current;
        //     set => hp.Current = value;
        // }
        //
        // [Networked] public int Attack { get; set; }
        // // 스텟 전체를 업데이트 ==>
        // // 
        //
        // [Networked]
        // public float AttackSpeed
        // {
        //     get => attackSpeed.Current;
        //     set => attackSpeed.Current = value;
        // }
        
        #endregion
        
        #endregion

        #region Variable Paramiter
        
        public bool IsDie => hp.isMin;

        #endregion

        #region Member Function

        public abstract void MainLoop();

        #endregion

        #region HP Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public virtual void ApplyDamageRPC(int damage, CrowdControl enemyProperty, RpcInfo info = default)
        {
            ApplyDamage(damage, enemyProperty);
            
            // TODO : Dubug 
            // DebugManager.ToDo("asdkasl");
            ShowInfo();
        }
        
        public abstract void ApplyDamage(int damage, CrowdControl enemyProperty);

        #endregion

        #region DeBug Function
        
        public abstract void ShowInfo();

        #endregion

        #region Condition Interface Functon

        // ICondition Interface Function
        public abstract bool On(CrowdControl condition);
            
        public abstract bool NormalityIsOn();
        public abstract bool PoisonedIsOn();
        public abstract bool WeakIsOn();
            
        public abstract void AddCondition(CrowdControl condition);
        public abstract void DelCondition(CrowdControl condition);
        
        #endregion
    }
}