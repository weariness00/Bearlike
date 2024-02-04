﻿using System;
using Fusion;
using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterState : Base.StateBase
    {
        public void Awake()
        {
            _hp.Max = 1000;
            _hp.Min = 0;
            _hp.Current = 1000;

            attack.Max = 100;
            attack.Min = 1;
            attack.Current = 10;

            defence.Max = 100;
            defence.Min = 1;
            defence.Current = 1;

            avoid.Max = 100.0f;
            avoid.Min = 0.0f;
            avoid.Current = 0.0f;
            
            moveSpeed.Max = 100;
            moveSpeed.Min = 1;
            moveSpeed.Current = 1;

            attackSpeed.Max = 10.0f;
            attackSpeed.Min = 0.5f;
            attackSpeed.Current = 1.0f;

            force.Max = 1000;
            force.Min = 0;
            force.Current = 10;
            
            condition = (int)ObjectProperty.Normality;
            property = (int)ObjectProperty.Normality;
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
            _hp.Current -= value;
        }

        public override bool ApplyDamage(float damage, ObjectProperty property)
        {
            if (_hp.Current < 0)
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

            _hp.Current -= (int)(damageRate * damage);

            if (_hp.Current == _hp.Min)
            {
                // 킬로그 구현할지 고민 (monster -> player)
                // respawn 시키는 코드 구현
            }
                
            return true;
        }

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  _hp.Current + $" 공격력 : " + attack.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (ObjectProperty)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }
        
        
        // ICondition Interface Function
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