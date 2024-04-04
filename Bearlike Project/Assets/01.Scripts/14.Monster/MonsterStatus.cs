using Manager;
using State.StateClass.Base;
using Status;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterStatus : StatusBase
    {
        public void Awake()
        {
            hp.Max = 100;
            hp.Min = 0;
            hp.Current = 100;

            damage.Max = 100;
            damage.Min = 1;
            damage.Current = 10;

            defence.Max = 100;
            defence.Min = 1;
            defence.Current = 10;

            avoid.Max = 100.0f;
            avoid.Min = 0.0f;
            avoid.Current = 0.0f;
            
            attackSpeed.Max = 10.0f;
            attackSpeed.Min = 0.5f;
            attackSpeed.Current = 1.0f;

            force.Max = 1000;
            force.Min = 0;
            force.Current = 10;
            
            condition = (int)CrowdControl.Normality;
            property = (int)CrowdControl.Normality;
        }

        private void Start()
        {
            InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }
        
        public override void MainLoop()
        {
            if (PoisonedIsOn())
            {
                BePoisoned(Define.PoisonDamage);
                ShowInfo();
            }
        }
        
        public void BePoisoned(int value)
        {
            hp.Current -= value;
        }
        
        public override void ApplyDamage(int damage, CrowdControl property)
        {
            if (hp.isMin)
            {
                return;
            }

            if ((Random.Range(0.0f, 99.9f) < avoid.Current))
            {
                return;
            }
            
            AddCondition(property);         // Monster의 속성을 Player상태에 적용
            
            var damageRate = math.log10((damage / defence.Current) * 10);

            if (WeakIsOn())
            {
                damageRate *= 1.5f;
            }
            
            hp.Current -= (int)(damageRate * damage);

            if (hp.Current == hp.Min)
            {
                // 킬로그 구현할지 고민 (monster -> player)
                // respawn 시키는 코드 구현
            }
                
            return;
        }

        // DeBug Function
        public override void ShowInfo()
        {
            DebugManager.Log($"체력 : " +  hp.Current + $" 공격력 : " + damage.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (CrowdControl)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }
        
        
        // ICondition Interface Function
        public override bool On(CrowdControl condition) { return (base.condition & (int)condition) == (int)condition; }

        public override bool NormalityIsOn() { return On(CrowdControl.Normality); }
        public override bool PoisonedIsOn() { return On(CrowdControl.Poisoned); }
        public override bool WeakIsOn() { return On(CrowdControl.Weak); }
        
        public override void AddCondition(CrowdControl condition)
        {
            if(!On(condition)) base.condition |= (int)condition;
        }
        
        public override void DelCondition(CrowdControl condition)
        {
            if(On(condition)) base.condition ^= (int)condition;
        }

        #region Json Data Interfacec

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
        }

        #endregion
    }
}