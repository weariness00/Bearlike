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
        
        public override void Awake()
        {
            base.Awake();
            var statusData = GetStatusData(id);
            _durationTime = statusData.GetFloat("Duration Time");
            
        }

        public override void Spawned()
        {
            base.Spawned();
            
            DurationTimeTimer = TickTimer.CreateFromTicks(Runner, 0);
        }
        
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                pc.status.AddAdditionalStatus(status);
            }
        }

        public override void MainLoop()
        {
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                isInvoke = false;
                SetSkillCoolTimerRPC(coolTime);

                status.avoidMultiple -= 0.3f;
            }
        }

        public override void Run()
        {
            if (IsUse && false == isInvoke)
            {
                StartVFXRPC();
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)
                
                status.avoidMultiple += 0.3f;
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }
    }
}