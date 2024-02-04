using System.Collections.Generic;
using Fusion;
using Scripts.State.GameStatus;
using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public class PlayerState : Base.StateBase
    {
        // Member Variable
        #region Info Perperty

        public StatusValue<int> level = new StatusValue<int>();               // 레벨
        public StatusValue<int> experience = new StatusValue<int>();                 // 경험치
        public List<int> experienceAmountList = new List<int>();  // 레벨별 경험치량
        public float immortalDurationAfterSpawn = 2f;           // 무적 시간
        
        #endregion
        
        #region Timer Property
        
        public GameObject immortalityIndicator;
        [Networked] private TickTimer _immortalTimer { get; set; }
        
        public bool IsImmortal => _immortalTimer.ExpiredOrNotRunning(Runner) == false;
        
        #endregion
        
        // Member Function
        // ObjectState abstract class Function
        void Awake()
        {
            hp.Max = 100;
            hp.Min = 0;
            hp.Current = 100;

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
            
            for(int i = 0; i < 10; ++i)
                experienceAmountList.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용
            
            level.Max = 10;
            level.Min = 1;
            level.Current = 1;

            experience.Max = experienceAmountList[level.Current];
            experience.Min = 0;
            experience.Current = 0;

            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }
        
        
        // Loop
        public override void MainLoop()
        {
            if (PoisonedIsOn())
            {
                BePoisoned(Define.PoisonDamage);
                ShowInfo();
            }
        }
        
        public override void Initialization()
        {
            // 혹시 모를 함수
        }
        // Loop
        

        // HP
        // 스킬, 무기, 캐릭터 스텟을 모두 고려한 함수 구현 필요
        public void BePoisoned(int value)
        {
            hp.Current -= value;
        }
        
        public override bool ApplyDamage(float damage, ObjectProperty monsterProperty) // MonsterRef instigator,
        {
            if (hp.Current < 0)
            {
                return false;
            }

            if (IsImmortal)
            {
                return false;
            }

            if ((Random.Range(0.0f, 99.9f) < avoid.Current))
            {
                return false;
            }
            
            AddCondition(monsterProperty);         // Monster의 속성을 Player상태에 적용
            
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
                
            return true;
        }
        // HP
        
        // LV
        public void IncreaseExp(int value)
        {
            experience.Current += value;

            while (experienceAmountList[level.Current] <= experience.Current && level.Max > level.Current)
            {
                experience.Current -= experienceAmountList[level.Current];
                level.Current++;
                experience.Max = experienceAmountList[level.Current];
                if(level.Max <= level.Current) Debug.Log("최대 레벨 도달");
            }
        }
        // LV
        public override void Spawned()
        {
            _immortalTimer = TickTimer.CreateFromSeconds(Runner, immortalDurationAfterSpawn);
        }

        public override void Render()
        {
            immortalityIndicator.SetActive(IsImmortal);
        }

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  hp.Current + $" 공격력 : " + attack.Current + $" 공격 속도 : " + attackSpeed.Current + $" 상태 : " + (ObjectProperty)condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
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