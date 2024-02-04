using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterState : Base.State
    {
        // Member Variable
        #region Info Perperty
        
        #endregion
        
        public override void Initialization()
        {
            
        }

        public override void MainLoop()
        {
            
        }

        public override bool ApplyDamage(float damage, ObjectProperty property)
        {
            if (hp.Current < 0)
            {
                return false;
            }

            if ((Random.Range(0.0f, 99.9f) < avoid.Current))
            {
                return false;
            }
            
            AddCondition(property);         // Monster의 속성을 Player상태에 적용
            
            var damageRate = math.log10((damage / defence.Current) * 10);

            if (WeakIsOn())
            {
                damageRate *= 1.5f;
            }

            hp.Current -= (int)(damageRate * damage);
                
            return true;
        }

        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  hp.Current + $" 공격력 : " + attack.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (ObjectProperty)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }

        public override bool On(ObjectProperty condition) { return (base.condition & (int)condition) == (int)condition; }

        public override bool NormalityIsOn() { return On(ObjectProperty.Normality); }
        public override bool PoisonedIsOn() { return On(ObjectProperty.Poisoned); }
        public override bool WeakIsOn() { return On(ObjectProperty.Weak); }
        
        public override void AddCondition(ObjectProperty condition)
        {
            if(!On(condition)) base.condition |= (int)condition;
        }
        
        public override void DelCondition(ObjectProperty condition)
        {
            if(On(condition)) base.condition ^= (int)condition;
        }
    }
}