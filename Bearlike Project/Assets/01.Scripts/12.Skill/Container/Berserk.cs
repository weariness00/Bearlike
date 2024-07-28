using Fusion;
using Manager;
using UnityEngine;
using Util;

namespace Skill.Container
{
    /// <summary>
    /// 몬슽터를 처치 시 공격력, 이동속도, 공격속도 증가
    /// 지속시간 내에 처치하면 스택이 쌓인다.
    /// </summary>
    public class Berserk : SkillBase
    {
        private float _amount; // N%만큼 강화
        private float _duration; // 지속시간
        private float _currentDurationTime;
        public TickTimer durationTimer;

        private int _comboCount = 0; // Monster 처치 콤보 Count

        private float _originDamageMultiple;
        private float _originAttackSpeedMultiple;
        private float _originMoveSpeedMultiple;

        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();
            var statusData = GetStatusData(id);

            _amount = statusData.GetFloat("Amount");
            _duration = statusData.GetFloat("Duration");
        }

        public override void Spawned()
        {
            base.Spawned();
            durationTimer = TickTimer.CreateFromSeconds(Runner,0);
        }

        #endregion

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            ownerPlayer.status.AddAdditionalStatus(status);
            ownerPlayer.MonsterKillAction -= MonsterKill;
            ownerPlayer.MonsterKillAction += MonsterKill;
            isInvoke = true;
        }

        public override void MainLoop()
        {
            _currentDurationTime += Runner.DeltaTime;
            if (durationTimer.Expired(Runner) == false)
            {
                var timeValue = _currentDurationTime / _duration;
                ownerPlayer.uiController.buffCanvas.SetTimer(skillName, timeValue);
            }
            else if (ownerPlayer.uiController.buffCanvas.HasUI(skillName))
            {
                ownerPlayer.uiController.buffCanvas.RemoveUI(skillName);
                status.attackSpeedMultiple = _originDamageMultiple;
                status.damageMultiple = _originAttackSpeedMultiple;
                status.moveSpeedMultiple = _originMoveSpeedMultiple;
            }
        }

        public override void Run(){}
        
        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Amount)"))
                explain = explain.Replace("(Amount)", $"({_amount * 100f})");
            if (explain.Contains("(Level)"))
                explain = explain.Replace("(Level)", $"{level.Current}");
            if (explain.Contains("(Duration)"))
                explain = explain.Replace("(Duration)", $"{_duration}");
            
            explain = StringExtension.CalculateNumber(explain);
        }

        // Monster를 처치시 발동
        public void MonsterKill(GameObject targetMonster)
        {
            if (durationTimer.Expired(Runner))
                _comboCount = 0;
            
            _currentDurationTime = 0;
            durationTimer = TickTimer.CreateFromSeconds(Runner,_duration);

            if (ownerPlayer.uiController.buffCanvas.HasUI(skillName) == false)
            {
                _originDamageMultiple = status.attackSpeedMultiple;
                _originAttackSpeedMultiple = status.damageMultiple;
                _originMoveSpeedMultiple = status.moveSpeedMultiple;
                
                ownerPlayer.uiController.buffCanvas.SpawnUI(skillName);
                ownerPlayer.uiController.buffCanvas.SetIcon(skillName, icon);
            }
            ownerPlayer.uiController.buffCanvas.SetStackText(skillName, _comboCount);
            
            ++_comboCount;
            var multiple = _comboCount * _amount;
            status.attackSpeedMultiple = _originDamageMultiple + multiple;
            status.damageMultiple = _originAttackSpeedMultiple + multiple;
            status.moveSpeedMultiple = _originMoveSpeedMultiple + multiple;
        }
    }
}