using Fusion;
using GamePlay;
using Manager;
using Player;
using Status;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 저격 소총 반자동 모드 : 저격 소총의 연사속도가 비약적으로 상승하는 스킬
    ///                      지속시간 : 7초, 재사용 대기시간 : 50초
    /// </summary>
    public class SniperContinuousMode : SkillBase
    {
        #region property
        
        private float _durationTime;
        
        [Networked] private TickTimer DurationTimeTimer { get; set; }
        
        #endregion

        private void Start()
        {
            base.Start();
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
                // TODO : 무기가 스나이퍼인지 확인해서 넣기
                pc.status.AddAdditionalStatus(status);
            }
        }
        
        public override void MainLoop()
        {
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                StopVFXRPC();
                isInvoke = false;
                SetSkillCoolTimerRPC(GetCoolTime());
                
                status.attackSpeedMultiple -= 1.0f;
            }
        }
    
        public override void Run()
        {
            if (IsUse && false == isInvoke)
            {
                StartVFXRPC();
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)

                status.attackSpeedMultiple += 1.0f;
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }

    }
}
