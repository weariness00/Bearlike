using System;
using System.Collections.Generic;
using Data;
using Fusion;
using Manager;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Status
{    
    /// <summary>
    /// Object의 상태를 나타내는 열거형
    /// </summary>
    public enum CrowdControl
    {
        Normality = 0b_0000_0000,           // 정상
        Poisoned = 0b_0000_0001,            // 중독 => 낮은 도트 데미지, 방어력 감소
        Weak = 0b_0000_0010,                // 취약 => 최종 데미지 1.5배 증가
        DamageIgnore = 0b_0000_0100,             // 방어 => 데미지 감소 || 무효
        Burn = 0b_0000_1000,                // 화상 => 높은 도트 데미지
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
        public StatusValue<int> damage = new StatusValue<int>(){Max = 99999};  // 공격력
        public float damageMultiple = 1; // 공격력 배율
        public float criticalHitMultiple = 1; // 치명타 배율
        public StatusValue<float> criticalHitChance = new StatusValue<float>(){Max = 1, isOverMax = true}; // 치명타 확률 0~1 값 1 이상이 될수도 있다.
        public StatusValue<int> defence = new StatusValue<int>(){Max = 99999};             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>(){Min = 0, Max = 1, isOverMax = true, isOverMin = true};           // 회피율 0 ~ 1 사이값
        public StatusValue<float> moveSpeed = new StatusValue<float>(){Max = 99999f};           // 이동 속도
        public StatusValue<float> attackSpeed = new StatusValue<float>(){Max = 99999f};     // 초당 공격 속도
        [Networked] public TickTimer AttackLateTimer { get; set; }
        public StatusValue<float> attackRange = new StatusValue<float>(){Max = 99999f};
        
        public int condition;                                                 // 상태
        public int property;                                                  // 속성
        public int burnDamage;
        public int poisonDamage;

        public bool IsDie => hp.isMin;
        
        #endregion

        #region Unity Evenet Function

        public override void Spawned()
        {
            AttackLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }

        public override void FixedUpdateNetwork()
        {
        }

        #endregion

        #region Member Function

        public virtual void MainLoop(){}

        public void ClearAdditionalStatus() => _additionalStatusList.Clear();
        public void AddAdditionalStatus(StatusBase otherStatus) => _additionalStatusList.Add(otherStatus);
        public void RemoveAdditionalStatus(StatusBase otherStatus) => _additionalStatusList.Remove(otherStatus);

        public virtual int CalDamage(int additionalDamage = 0, float additionalDamageMultiple = 0f, float additionalCriticalHitMultiple = 0f)
        {
            var d = AddAllDamage() + additionalDamage;
            var dm = AddAllDamageMagnification() + 1 + additionalDamageMultiple;
            var chm = CalCriticalHit() + additionalCriticalHitMultiple;

            return (int)Math.Round(chm * dm * d);
        }

        public float CalCriticalHit()
        {
            float chm = AddAllCriticalHitMultiple();
            float chc = AddAllCriticalHitChance();

            float resultCHM = 1;
            while (true)
            {
                if (Random.Range(0f, 1f) < chc)
                {
                    resultCHM += chm;
                    chc -= 1f;
                }
                else
                {
                    break;
                }//
            }

            return resultCHM;
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
            float dm = damageMultiple - 1;
            foreach (var statusBase in _additionalStatusList)
            {
                dm += statusBase.AddAllDamageMagnification();
            }

            return dm;
        }

        private float AddAllCriticalHitMultiple()
        {
            float chm = criticalHitMultiple - 1;
            foreach (var statusBase in _additionalStatusList)
            {
                chm += statusBase.AddAllCriticalHitMultiple();
            }

            return chm;
        }
        
        private float AddAllCriticalHitChance()
        {
            float chc = criticalHitChance;
            foreach (var statusBase in _additionalStatusList)
            {
                chc += statusBase.AddAllCriticalHitChance();
            }
            return chc;
        }

        public virtual void ApplyDamage(int applyDamage, NetworkId ownerId, CrowdControl cc)
        {
            if (hp.isMin)
            {
                return;
            }

            if (Random.Range(0f, 1f) < avoid.Current)
            {
                DebugManager.Log($"{name} 회피 성공");
                return;
            }

            if (!ConditionDamageIgnoreIsOn())
            {
                AddCondition(cc); // Monster의 속성을 Player상태에 적용

                var damageRate = 1f;

                if (ConditionWeakIsOn())
                {
                    damageRate *= 1.5f;
                }

                hp.Current -= (int)(damageRate * applyDamage);
                DebugManager.Log($"{gameObject.name}에게 {damageRate * applyDamage}만큼 데미지\n" +
                    $"남은 hp : {hp.Current}");
            }
        }

        // 상태이상 적용
        public void ApplyCrowdControl()
        {
            
        }

        public virtual void ShowInfo()
        {
            DebugManager.Log($"{gameObject.name} - 체력 : " +  hp.Current + $" 공격력 : " + damage.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (CrowdControl)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }

        #endregion

        #region Condition Interface Functon

        // ICondition Interface Function

        #region Condition

        public bool ConditionOn(CrowdControl cc)
        {
            return (condition & (int)cc) == (int)cc;
        }
        // ICondition Interface Function
        public bool ConditionNormalityIsOn() { return ConditionOn(CrowdControl.Normality); }
        public bool ConditionPoisonedIsOn() { return ConditionOn(CrowdControl.Poisoned); }
        public bool ConditionWeakIsOn() { return ConditionOn(CrowdControl.Weak); }
        public bool ConditionDamageIgnoreIsOn() { return ConditionOn(CrowdControl.DamageIgnore); }

        public void AddCondition(CrowdControl cc)
        {
            if(!ConditionOn(cc)) condition |= (int)cc;
        }
        
        public void DelCondition(CrowdControl cc)
        {
            if(ConditionOn(cc)) condition ^= (int)cc;
        }

        #endregion

        #region Property

        // public bool PropertyOn(CrowdControl cc)
        // {
        //     return (property & (int)cc) == (int)cc;
        // }

        #endregion
        
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

            if(json.HasFloat("Damage Multiple")) damageMultiple = json.GetFloat("Damage Multiple");
            if(json.HasFloat("CriticalHit Multiple")) damageMultiple = json.GetFloat("CriticalHit Multiple");
            criticalHitChance.Current = json.GetFloat("CriticalHit Chance");
            
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

            poisonDamage = json.GetInt("Poison Damage");
            poisonDamage = json.GetInt("Burn Damage");
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
                    break;
                case StatusValueType.Max:
                    attackSpeed.Max = value;
                    break;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void StartAttackTimerRPC()
        {
            AttackLateTimer = TickTimer.CreateFromSeconds(Runner, 1f / attackSpeed.Current);
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
        }

        #endregion
    }
}