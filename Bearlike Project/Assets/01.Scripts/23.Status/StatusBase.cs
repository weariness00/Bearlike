using _23.Status;
using Data;
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
    public abstract class StatusBase : NetworkBehaviour, IJsonData<StatusJsonData>
    {
        #region Member Variable
        
        public StatusValue<int> hp = new StatusValue<int>();                  // 체력        
        public StatusValue<int> damage = new StatusValue<int>();              // 공격력
        public StatusValue<int> defence = new StatusValue<int>();             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>();           // 회피율 0 ~ 1 사이값
        public StatusValue<int> moveSpeed = new StatusValue<int>();           // 이동 속도
        public StatusValue<float> attackSpeed = new StatusValue<float>();     // 초당 공격 속도
        public StatusValue<float> attackLateTime = new StatusValue<float>();  // Attack Speed에 따른 딜레이
        public StatusValue<float> attackRange = new StatusValue<float>();
        
        public StatusValue<int> force = new StatusValue<int>();               // 힘
        public int condition;                                                 // 상태
        public int property;                                                  // 속성

        public bool IsDie => hp.isMin;
        
        #endregion

        #region Unity Evenet Function

        public override void Spawned()
        {
            attackLateTime.Max = 1 / attackSpeed.Current;
        }

        public override void FixedUpdateNetwork()
        {
            attackLateTime.Current += Runner.DeltaTime;
        }

        #endregion

        #region Member Function

        public abstract void MainLoop();
        public abstract void ApplyDamage(int damage, CrowdControl enemyProperty);
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

        #region Json Data Interface

        public StatusJsonData GetJsonData()
        {
            return new StatusJsonData();
        }

        public void SetJsonData(StatusJsonData json)
        {
            hp.Max = json.GetInt("Hp Max");
            hp.Current = json.GetInt("Hp Current");
            
            damage.Max = json.GetInt("Damage Max");
            damage.Min = json.GetInt("Damage Min");
            damage.Current = json.GetInt("Damage Current");
            
            defence.Max = json.GetInt("Defence Max");
            defence.Min = json.GetInt("Defence Min");
            defence.Current = json.GetInt("Defence Current");
            
            avoid.Max = json.GetFloat("Avoid Max");
            avoid.Min = json.GetFloat("Avoid Min");
            avoid.Current = json.GetFloat("Avoid Current");
            
            moveSpeed.Max = json.GetInt("MoveSpeed Max");
            moveSpeed.Current = json.GetInt("MoveSpeed Current");
            
            attackSpeed.Max = json.GetFloat("AttackSpeed Max");
            attackSpeed.Current = json.GetFloat("AttackSpeed Current");
            
            attackRange.Max = json.GetFloat("AttackRange Max");
            attackRange.Current = json.GetFloat("AttackRange Current");

            property = json.GetInt("Property");
            condition = json.GetInt("Condition");
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetHpRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Min:
                    hp.Min = value;
                    break;
                case StatusValueType.Current:
                    hp.Current = value;
                    break;
                case StatusValueType.Max:
                    hp.Max = value;
                    break;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetAttackSpeedRPC(StatusValueType type, float value)
        {
            switch (type)
            {
                // min 값은 0으로 고정이기에 변하면 안된다.
                case StatusValueType.Current:
                    attackSpeed.Current = value;
                    attackLateTime.Max = 1 / attackSpeed.Current;
                    break;
                case StatusValueType.Max:
                    attackSpeed.Max = value;
                    break;
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ApplyDamageRPC(int damage, CrowdControl enemyProperty = CrowdControl.Normality, RpcInfo info = default)
        {
            ApplyDamage(damage, enemyProperty);
            ShowInfo();
        }

        #endregion
    }
}