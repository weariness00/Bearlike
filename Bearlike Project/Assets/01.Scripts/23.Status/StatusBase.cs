using Data;
using Fusion;
using Manager;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Status
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
    public class StatusBase : NetworkBehaviour, IJsonData<StatusJsonData>
    {
        #region Member Variable
        
        public StatusValue<int> hp = new StatusValue<int>();                  // 체력        
        public StatusValue<int> damage = new StatusValue<int>();              // 공격력
        public StatusValue<int> defence = new StatusValue<int>();             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>(){Min = 0, Max = 1, isOverMax = true, isOverMin = true};           // 회피율 0 ~ 1 사이값
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
            attackLateTime.Max = 1 / attackSpeed.Current == 0 ? 1 : attackSpeed.Current;
        }

        public override void FixedUpdateNetwork()
        {
            attackLateTime.Current += Runner.DeltaTime;
        }

        #endregion

        #region Member Function

        public virtual void MainLoop(){}

        public virtual void ApplyDamage(int applyDamage, CrowdControl cc)
        {
            if (hp.isMin)
            {
                return;
            }

            if ((Random.Range(0f, 1f) < avoid.Current))
            {
                return;
            }
            
            AddCondition(cc);         // Monster의 속성을 Player상태에 적용
            
            var damageRate = math.log10((applyDamage / defence.Current) * 10);

            if (WeakIsOn())
            {
                damageRate *= 1.5f;
            }
            
            hp.Current -= (int)(damageRate * applyDamage);
        }

        public virtual void ShowInfo()
        {
            DebugManager.Log($"{gameObject.name} - 체력 : " +  hp.Current + $" 공격력 : " + damage.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (CrowdControl)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }

        #endregion

        #region Condition Interface Functon

        // ICondition Interface Function
        public virtual bool On(CrowdControl cc)
        {
            return (condition & (int)cc) == (int)cc;
        }
        // ICondition Interface Function
        public virtual bool NormalityIsOn() { return On(CrowdControl.Normality); }
        public virtual bool PoisonedIsOn() { return On(CrowdControl.Poisoned); }
        public virtual bool WeakIsOn() { return On(CrowdControl.Weak); }

        public virtual void AddCondition(CrowdControl cc)
        {
            if(!On(cc)) condition |= (int)cc;
        }
        
        public virtual void DelCondition(CrowdControl cc)
        {
            if(On(cc)) condition ^= (int)cc;
        }
        #endregion

        #region Json Data Interface

        public virtual StatusJsonData GetJsonData()
        {
            return new StatusJsonData();
        }

        public virtual void SetJsonData(StatusJsonData json)
        {
            hp.Max = json.GetInt("Hp Max");
            hp.Current = json.GetInt("Hp Current");
            
            damage.Max = json.GetInt("Damage Max");
            damage.Min = json.GetInt("Damage Min");
            damage.Current = json.GetInt("Damage Current");
            
            defence.Max = json.GetInt("Defence Max");
            defence.Min = json.GetInt("Defence Min");
            defence.Current = json.GetInt("Defence Current");
            
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
                
                case StatusValueType.CurrentAndMax:
                    hp.Max = value;
                    hp.Current = value;
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