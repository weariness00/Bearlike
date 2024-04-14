using System;
using System.Collections.Generic;
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

        // 추가적인 스테이터스
        // ex ) Gun은 Player의 소유 Gun에서 나가는 Bullet에는 Player와 Gun의 스텟이 필요 그때마다 불러오기가 힘드니 Gun의 추가적인 Status에 Player의 Status를 포함
        private HashSet<StatusBase> _additionalStatusList = new HashSet<StatusBase>();
        
        public StatusValue<int> hp = new StatusValue<int>(){Max = 99999};                  // 체력        
        public StatusValue<int> damage = new StatusValue<int>(){Max = 99999};              // 공격력
        public float damageMultiple = 1; // 공격력 배율
        public StatusValue<int> defence = new StatusValue<int>(){Max = 99999};             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>(){Min = 0, Max = 1, isOverMax = true, isOverMin = true};           // 회피율 0 ~ 1 사이값
        public StatusValue<float> moveSpeed = new StatusValue<float>(){Max = 99999f};           // 이동 속도
        public StatusValue<float> attackSpeed = new StatusValue<float>(){Max = 99999f};     // 초당 공격 속도
        public StatusValue<float> attackLateTime = new StatusValue<float>();  // Attack Speed에 따른 딜레이
        public StatusValue<float> attackRange = new StatusValue<float>(){Max = 99999f};
        
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

        public void ClearAdditionalStatus() => _additionalStatusList.Clear();
        public void AddAdditionalStatus(StatusBase otherStatus)
        {
            _additionalStatusList.Add(otherStatus);
        }
        
        public virtual int CalDamage()
        {
            var d = AddAllDamage();
            var dm = AddAllDamageMagnification();

            return (int)Math.Round(dm * d);
        }

        private int AddAllDamage()
        {
            var d = damage.Current;
            foreach (var statusBase in _additionalStatusList)
            {
                d += statusBase.AddAllDamage();
            }

            return d;
        }

        private float AddAllDamageMagnification()
        {
            var dm = damageMultiple;
            foreach (var statusBase in _additionalStatusList)
            {
                dm += statusBase.AddAllDamageMagnification();
            }

            return dm;
        }

        public virtual void ApplyDamage(int applyDamage, NetworkId ownerId, CrowdControl cc)
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

            if(json.HasFloat("Damage Magnification")) damageMultiple = json.GetFloat("Damage Magnification");
            
            defence.Max = json.GetInt("Defence Max");
            defence.Min = json.GetInt("Defence Min");
            defence.Current = json.GetInt("Defence Current");
            
            avoid.Current = json.GetFloat("Avoid Current");
            
            moveSpeed.Max = json.GetFloat("MoveSpeed Max");
            moveSpeed.Current = json.GetFloat("MoveSpeed Current");
            
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="id">대미지를 준 대상의 Network ID</param>
        /// <param name="enemyProperty"></param>
        /// <param name="info"></param>
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ApplyDamageRPC(int damage, NetworkId id, CrowdControl enemyProperty = CrowdControl.Normality, RpcInfo info = default)
        {
            ApplyDamage(damage, id, enemyProperty);
            ShowInfo();
        }

        #endregion
    }
}