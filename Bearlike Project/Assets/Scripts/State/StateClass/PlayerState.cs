using System.Collections.Generic;
using Fusion;
using Scripts.State.GameStatus;
using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public class PlayerState : Base.State
    {
        // Member Variable
        #region info
        
        public StatusValue<int> Level { get; set; }               // 레벨
        public StatusValue<int> Experience { get; set; }                 // 경험치
        public List<int> ExperienceAmountList { get; set; }     // 레벨별 경험치량
        public float immortalDurationAfterSpawn = 2f;           // 무적 시간
        
        #endregion
        
        #region timer
        
        public GameObject immortalityIndicator;
        [Networked]
        private TickTimer _immortalTimer { get; set; }
        
        public bool IsImmortal => _immortalTimer.ExpiredOrNotRunning(Runner) == false;
        
        #endregion
        
        // Member Function
        // ObjectState abstract class Function
        public PlayerState()
        {
            Hp.Max = 100;
            Hp.Min = 0;
            Hp.Current = 100;

            Attack.Max = 100;
            Attack.Min = 1;
            Attack.Current = 10;

            Defence.Max = 100;
            Defence.Min = 1;
            Defence.Current = 1;

            Avoid.Max = 100.0f;
            Avoid.Min = 0.0f;
            Avoid.Current = 0.0f;
            
            MoveSpeed.Max = 100;
            MoveSpeed.Min = 1;
            MoveSpeed.Current = 1;

            AttackSpeed.Max = 10.0f;
            AttackSpeed.Min = 0.5f;
            AttackSpeed.Current = 1.0f;

            Force.Max = 1000;
            Force.Min = 0;
            Force.Current = 10;
            
            Condition = (int)ObjectProperty.Normality;
            
            for(int i = 0; i < 10; ++i)
                ExperienceAmountList.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용
            
            Level.Max = 10;
            Level.Min = 1;
            Level.Current = 1;

            Experience.Max = ExperienceAmountList[Level.Current];
            Experience.Min = 0;
            Experience.Current = 0;

            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }
        
        
        // Loop
        public override void MainLoop()
        {
            if (PoisonedIsOn())
            {
                BePoisoned(Constants.POISONDAMAGE);
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
            Hp.Current -= value;
        }
        
        public override bool ApplyDamage(float damage, ObjectProperty monsterProperty) // MonsterRef instigator,
        {
            if (Hp.Current < 0)
            {
                return false;
            }

            if (IsImmortal)
            {
                return false;
            }

            if ((Random.Range(0.0f, 99.9f) < Avoid.Current))
            {
                return false;
            }
            
            AddCondition(monsterProperty);         // Monster의 속성을 Player상태에 적용
            
            var damageRate = math.log10((damage / Defence.Current) * 10);

            if (WeakIsOn())
            {
                damageRate *= 1.5f;
            }

            Hp.Current -= (int)(damageRate * damage);

            if (Hp.Current == Hp.Min)
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
            Experience.Current += value;

            while (ExperienceAmountList[Level.Current] <= Experience.Current && Level.Max > Level.Current)
            {
                Experience.Current -= ExperienceAmountList[Level.Current];
                Level.Current++;
                Experience.Max = ExperienceAmountList[Level.Current];
                if(Level.Max <= Level.Current) Debug.Log("최대 레벨 도달");
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
            Debug.Log($"체력 : " +  Hp.Current + $" 공격력 : " + Attack.Current + $" 공격 속도 : " + AttackSpeed.Current + $" 상태 : " + (ObjectProperty)Condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }
        
        
        // ICondition Interface Function
        public override bool On(ObjectProperty condition) { return (Condition & (int)condition) == (int)condition; }

        public override bool NormalityIsOn() { return On(ObjectProperty.Normality); }
        public override bool PoisonedIsOn() { return On(ObjectProperty.Poisoned); }
        public override bool WeakIsOn() { return On(ObjectProperty.Weak); }
        
        public override void AddCondition(ObjectProperty condition)
        {
            if(!On(condition)) Condition |= (int)condition;
        }
        
        public override void DelCondition(ObjectProperty condition)
        {
            if(On(condition)) Condition ^= (int)condition;
        }
    }
}