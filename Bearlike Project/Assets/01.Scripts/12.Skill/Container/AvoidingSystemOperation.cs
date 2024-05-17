using Fusion;
using GamePlay;
using Player;
using Status;
using UnityEngine;

namespace Skill.Container
{
    /// <summary>
    /// 회피 시스템 작동 : 스킬을 사용하면 10초간 회피률이 1.3배 상승한다.
    ///                  지속 시간 : 10초 / 재사용 대기 시간 : 30초
    /// </summary>
    public sealed class AvoidingSystemOperation : SkillBase
    {
        #region property
        
        private float _durationTime;
        
        [Networked] private TickTimer DurationTimeTimer { get; set; }

        #endregion
        
        #region Value

        public float duration = 10.0f;
        private const float AvoidValue = 0.3f;
        private const float CoolTime = 30.0f;
        [Networked] private TickTimer DurationTimer { get; set; }
        private const float DurationTime = 10.0f;

        #endregion
        
        public override void Awake()
        {
            base.Start();
            var statusData = GetStatusData(id);
            _durationTime = statusData.GetFloat("Duration Time");
            
        }

        public override void Spawned()
        {
            base.Spawned();
            
            DurationTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        public override void MainLoop()
        {

        }

        public override void Run()
        {

        }
    }
}