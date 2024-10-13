using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using Fusion;
using Manager;
using Player;
using UI.Status;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Status
{    
    /// <summary>
    /// Object의 상태를 나타내는 열거형
    /// </summary>
    [Serializable]
    public enum CrowdControl
    {
        Normality = 0b_0000_0000,           // 정상
        Poisoned = 0b_0000_0001,            // 중독 => 낮은 도트 데미지, 방어력 감소
        Weak = 0b_0000_0010,                // 취약 => 최종 데미지 1.5배 증가
        DamageIgnore = 0b_0000_0100,             // 방어 => 데미지 감소 || 무효
        Burn = 0b_0000_1000,                // 화상 => 높은 도트 데미지
        DamageReflect = 0b_0001_0000,       // 반사 => 데미지를 특정 비율로 반사
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
        public float damageMultiple = 1f; // 공격력 배율
        public StatusValue<float> criticalHitChance = new StatusValue<float>(){Max = 1, isOverMax = true}; // 치명타 확률 0~1 값 1 이상이 될수도 있다.
        public float criticalHitMultiple = 1f; // 치명타 배율
        public StatusValue<int> defence = new StatusValue<int>(){Max = 99999};             // 방어력
        public StatusValue<float> avoid = new StatusValue<float>(){Min = 0, Max = 1, isOverMax = true, isOverMin = true};           // 회피율 0 ~ 1 사이값
        public float avoidMultiple = 1f;
        public StatusValue<float> moveSpeed = new StatusValue<float>(){Max = 99999f};           // 이동 속도
        public float moveSpeedMultiple = 1f;
        public StatusValue<float> attackSpeed = new StatusValue<float>(){Max = 99999f};     // 초당 공격 속도
        public float attackSpeedMultiple = 1f;   // 공격 속도 배율
        [Networked] public TickTimer AttackLateTimer { get; set; }
        public StatusValue<float> attackRange = new StatusValue<float>(){Max = 99999f};
        
        public int condition;                                                 // 상태
        public int property;                                                  // 속성
        public int burnDamage;
        public int poisonDamage;

        public int knockBack = 0;    // 넉백 속성
        
        public bool IsDie => hp.isMin;

        private Func<int, int> _beforeApplyDamage; // 대미지 적용 직전 이벤트

        #endregion

        #region Unity Evenet Function

        public override void Spawned()
        {
            AttackLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }
        
        #endregion

        #region Member Function

        public virtual void MainLoop(){}

        public void ClearAdditionalStatus() => _additionalStatusList.Clear();
        public void AddAdditionalStatus(StatusBase otherStatus) => _additionalStatusList.Add(otherStatus);
        public void RemoveAdditionalStatus(StatusBase otherStatus) => _additionalStatusList.Remove(otherStatus);

        /// <summary>
        /// 대미지를 적용하기 직전에 동작하는 이벤트를 추가
        /// 반환형 : int, 인자 int ApplyDamage
        /// </summary>
        /// <param name="func"></param>
        /// <param name="isPermitDuplication">이벤트 추가할때 중복된 이벤트가 있어도 추가할지에 대한 여부</param>
        public void AddBeforeApplyDamageEvent(Func<int, int> func, bool isPermitDuplication = true)
        {
            if (isPermitDuplication == false)
            {
                if (_beforeApplyDamage != null)
                {
                    bool isIncluded = false;
                    foreach (var @delegate in _beforeApplyDamage.GetInvocationList())
                    {
                        var includeFunc = (Func<int, int>)@delegate;
                        if (includeFunc.Method == func.Method)
                        {
                            isIncluded = true;
                            break;
                        }
                    }

                    if (isIncluded == false)
                    {
                        _beforeApplyDamage += func;
                    }
                }
                else
                {
                    _beforeApplyDamage += func;
                }
            }
            else
            {
                _beforeApplyDamage += func;
            }
        }
        public void RemoveBeforeApplyDamageEvent(Func<int, int> func) => _beforeApplyDamage -= func;
        
        #region Damage

        public virtual int CalDamage(out bool isCritical, int additionalDamage = 0, float additionalDamageMultiple = 0f, float additionalCriticalHitMultiple = 0f)
        {
            var d = AddAllDamage() + additionalDamage;
            var dm = AddAllDamageMagnification() + 1 + additionalDamageMultiple;
            var chm = CalCriticalHit() + additionalCriticalHitMultiple;

            isCritical = !chm.Equals(1f);
            
            if (dm < 0) return 0; // 대미지 배율이 -이면 대미지는 0이다.
            return (int)Math.Round(chm * dm * d);
        }

        private float CalCriticalHit()
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
        
        public int AddAllDamage()
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
        
        #endregion

        #region Attack Speed
        
        public virtual int CalAttackSpeed(int additionalAttackSpeed = 0, float additionalAttackSpeedMultiple = 0f)
        {
            var ats = AddAllAttackSpeed() + additionalAttackSpeed;
            var atsm = AddAllAttackSpeedMultiple() + 1 + additionalAttackSpeedMultiple;

            return (int)Math.Round(atsm * ats);
        }

        private float AddAllAttackSpeed()
        {
            var value = attackSpeed.Current;
            foreach (var statusBase in _additionalStatusList)
            {
                value += statusBase.AddAllAttackSpeed();
            }
            return value;
        }
        
        private float AddAllAttackSpeedMultiple()
        {
            float asm = attackSpeedMultiple - 1;
            
            // TODO : asm의 값을 체크해주는 방법 생각
            if (asm < 0)
                asm = 0;
            foreach (var statusBase in _additionalStatusList)
            {
                asm += statusBase.AddAllAttackSpeedMultiple();
            }

            return asm;
        }
        
        #endregion

        #region Move Speed
        
        public virtual float GetMoveSpeed()
        {
            var ms = AddAllMoveSpeed();
            var msm = AddAllMoveSpeedMultiple() + 1;

            return ms * msm;
        }

        private float AddAllMoveSpeed()
        {
            var ms = moveSpeed.Current;
            foreach (var statusBase in _additionalStatusList)
                ms += statusBase.AddAllMoveSpeed();

            return ms;
        }

        private float AddAllMoveSpeedMultiple()
        {
            float msm = moveSpeedMultiple - 1;
            foreach (var statusBase in _additionalStatusList)
                msm += statusBase.AddAllMoveSpeedMultiple();

            return msm;
        }
        
        #endregion

        public int GetAllNuckBack()
        {
            int nb = knockBack;
            foreach (var statusBase in _additionalStatusList)
            {
                var value = statusBase.GetAllNuckBack();
                if (nb < value)
                    nb = value;
            }

            return nb;
        }

        public virtual void ApplyDamage(int applyDamage, DamageTextType damageType, NetworkId ownerId, CrowdControl cc)
        {
            if (hp.isMin)
            {
                return;
            }

            // if (Random.Range(0f, 1f) < avoid.Current)
            // {
            //     DebugManager.Log($"{name} 회피 성공");
            //     return;
            // }

            if (!ConditionDamageIgnoreIsOn())
            {
                if (_beforeApplyDamage != null)
                {
                    foreach (var @delegate in _beforeApplyDamage.GetInvocationList())
                    {
                        var func = (Func<int, int>)@delegate;
                        applyDamage = func(applyDamage);
                    }
                }
                
                AddCondition(cc); // Monster의 속성을 Player상태에 적용

                var damageRate = math.clamp(math.log10((applyDamage / (float)(defence * 2)) * 10), 0.0f, 1.0f);
                
                damageRate = 1f;
                
                if (ConditionWeakIsOn())
                {
                    damageRate *= 1.5f;
                }

                var realDamage = (int)(damageRate * applyDamage);
                hp.Current -= realDamage;
                DamageText(realDamage, damageType);

                { // 대미지를 입고 난 후에 이벤트 OwnerId에 해당하는 Object가 소지한 이벤트를 발동시킨다.
                    var ownerObj = Runner.FindObject(ownerId);
                    if (ownerObj)
                    {
                        if (ownerObj.TryGetComponent(out IAfterApplyDamage afterApplyDamage))
                        {
                            afterApplyDamage.AfterApplyDamageAction?.Invoke(realDamage);
                        }
                        
                        if (HasStateAuthority && ConditionDamageReflectIsOn())
                        {
                            var playerStatus = ownerObj.gameObject.GetComponent<PlayerStatus>();

                            playerStatus.ApplyDamageRPC(realDamage / 10, DamageTextType.Normal, Object.Id, CrowdControl.Normality);
                        
                            DebugManager.Log($"{ownerObj.name} player가 반사로 인해 {realDamage / 10}만큼 데미지를 받음\n"+
                                             $"남은 hp : {playerStatus.hp.Current}");
                        }
                    }
                }
                
                DebugManager.Log(
                    $"{gameObject.name}에게 {damageRate * applyDamage}만큼 데미지\n" +
                    $"남은 hp : {hp.Current}");
            }
        }

        public virtual void ApplyHeal(int applyHeal, NetworkId ownerId, CrowdControl cc = CrowdControl.Normality)
        {
            if (hp.isMax)
                return;

            hp.Current += applyHeal;
            HealingText(applyHeal);
            
            DebugManager.Log(
                $"{gameObject.name}에게 {applyHeal}만큼 체력 회복\n" +
                $"남은 hp : {hp.Current}");
        }

        public void KnockBack()
        {
            
        }
        
        public virtual void DamageText(int realDamage, DamageTextType type){}
        public virtual void HealingText(int realHealAmount) {}

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
        public bool ConditionDamageReflectIsOn() { return ConditionOn(CrowdControl.DamageReflect); }

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
            if(json.HasInt("Hp Max")) hp.Max = json.GetInt("Hp Max");
            if(json.HasInt("Hp Max")) hp.Current = json.GetInt("Hp Current");
            
            if(json.HasInt("Damage Max")) damage.Max = json.GetInt("Damage Max");
            if(json.HasInt("Damage Min")) damage.Min = json.GetInt("Damage Min");
            if(json.HasInt("Damage Current")) damage.Current = json.GetInt("Damage Current");

            if(json.HasFloat("Damage Multiple")) damageMultiple = json.GetFloat("Damage Multiple");
            if(json.HasFloat("CriticalHit Multiple")) damageMultiple = json.GetFloat("CriticalHit Multiple");
            criticalHitChance.Current = json.GetFloat("CriticalHit Chance");
            
            if(json.HasInt("Defence Max")) defence.Max = json.GetInt("Defence Max");
            if(json.HasInt("Defence Min")) defence.Min = json.GetInt("Defence Min");
            if(json.HasInt("Defence Current")) defence.Current = json.GetInt("Defence Current");
            
            avoid.Current = json.GetFloat("Avoid Current");
            
            moveSpeed.Max = json.GetFloat("MoveSpeed Max");
            moveSpeed.Current = json.GetFloat("MoveSpeed Current");
            
            attackSpeed.Max = json.GetFloat("AttackSpeed Max");
            attackSpeed.Current = json.GetFloat("AttackSpeed Current");
            if(json.HasFloat("AttackSpeed Multiple")) attackSpeedMultiple = json.GetFloat("AttackSpeed Multiple");
            
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
        
        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void ApplyDamageRPC(int applyDamage, DamageTextType damageType, NetworkId id, CrowdControl enemyProperty = CrowdControl.Normality, RpcInfo info = default)
        {
            ApplyDamage(applyDamage, damageType, id, enemyProperty);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void ApplyHealRPC(int heal, NetworkId id, CrowdControl enemyProperty = CrowdControl.Normality, RpcInfo info = default)
        {
            ApplyHeal(heal, id, enemyProperty);
        }

        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void KnockBackRPC(Vector3 direction, int amount)
        {
            KnockBack();
                        
            UnityEngine.AI.NavMeshAgent _enemynav = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            if(_enemynav != null) _enemynav.enabled = false;

            // transform.DOMove(transform.position + direction * amount * 5, 0.5f)
            //     .SetEase(Ease.OutCirc);
            StartCoroutine(NuckNackCoroutine(_enemynav));
        }

        IEnumerator NuckNackCoroutine(UnityEngine.AI.NavMeshAgent _nav)
        {
            float time = 0.0f;

            while (time < 1.0f)
            {
                transform.position = transform.position;
                
                time += 2 * Time.deltaTime;
                yield return null;
            }
            
            if(_nav != null) _nav.enabled = true;
        }

        #endregion
    }
}