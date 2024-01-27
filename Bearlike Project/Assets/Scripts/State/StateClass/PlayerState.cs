using System.Collections.Generic;
using Scripts.State.GameStatus;
using State.StateClass.Pure;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public class PlayerState : Pure.State
    {
        #region Public Parameter

        #endregion
        
        // Member Variable
        private StatusValue<int> _level = new StatusValue<int>();               // 레벨
        private StatusValue<int> _exp = new StatusValue<int>();                 // 경험치
        private List<int> _expAmount = new List<int>();                         // 레벨별 경험치량
        
        // Member Function
        // ObjectState abstract class Function
        public PlayerState()
        {
            _hp.Max = 100;
            _hp.Min = 0;
            _hp.Current = 100;

            _attack.Max = 100;
            _attack.Min = 1;
            _attack.Current = 10;

            _defence.Max = 100;
            _defence.Min = 1;
            _defence.Current = 1;

            _avoid.Max = 100.0f;
            _avoid.Min = 0.0f;
            _avoid.Current = 0.0f;
            
            _moveSpeed.Max = 100;
            _moveSpeed.Min = 1;
            _moveSpeed.Current = 1;

            _attackSpeed.Max = 10.0f;
            _attackSpeed.Min = 0.5f;
            _attackSpeed.Current = 1.0f;

            _force.Max = 1000;
            _force.Min = 0;
            _force.Current = 10;
            
            _condition = (int)eCondition.Normality;
            
            for(int i = 0; i < 10; ++i)
                _expAmount.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용
            
            _level.Max = 10;
            _level.Min = 1;
            _level.Current = 1;

            _exp.Max = _expAmount[_level.Current];
            _exp.Min = 0;
            _exp.Current = 0;

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
        // // 스킬, 무기, 캐릭터 스텟을 모두 고려한 함수 구현 필요
        public void BePoisoned(int value)
        {
            _hp.Current -= value;
        }
        
        public override void BeDamaged(float damage)
        {   
            if ((Random.Range(0.0f, 99.9f) < _avoid.Current)) return;
            
            var damageRate = math.log10((damage / _defence.Current) * 10);

            if (WeakIsOn()) damageRate *= 1.5f;

            _hp.Current -= (int)(damageRate * damage);
        }
        // HP
        
        // LV
        public void IncreaseExp(int value)
        {
            _exp.Current += value;

            while (_expAmount[_level.Current] <= _exp.Current && _level.Max > _level.Current)
            {
                _exp.Current -= _expAmount[_level.Current];
                _level.Current++;
                _exp.Max = _expAmount[_level.Current];
                if(_level.Max <= _level.Current) Debug.Log("최대 레벨 도달");
            }
        }
        // LV

        // DeBug Function
        public override void ShowInfo()
        {
            Debug.Log($"체력 : " +  _hp.Current + $" 공격력 : " + _attack.Current + $" 공격 속도 : " + _attackSpeed.Current + $" 상태 : " + (eCondition)_condition);    // condition이 2개 이상인 경우에는 어떻게 출력?
        }
        
        
        // ICondition Interface Function
        public override bool On(int condition) { return (_condition & condition) == condition; }

        public override bool NormalityIsOn() { return On((int)eCondition.Normality); }
        public override bool PoisonedIsOn() { return On((int)eCondition.Poisoned); }
        public override bool WeakIsOn() { return On((int)eCondition.Weak); }
        
        public override void AddCondition(int condition)
        {
            if(!On(condition)) _condition |= condition;
        }
        
        public override void DelCondition(int condition)
        {
            if(On(condition)) _condition ^= condition;
        }
    }
}