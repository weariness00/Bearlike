using Fusion;
using Player;
using Status;
using UnityEngine;

namespace Skill.Container
{
    /// <summary>
    /// 샷건의 산탄 총알들이 강력해져서 몬스터들은 뒤로 밀도록 개조하는 스킬
    /// 쿨타임 - 30초, 지속 시간 : 10초 (패시브로 할까 생각중) 
    /// </summary>
    public class KnockbackShot : SkillBase
    {
        
        private float _durationTime;
        private TickTimer DurationTimeTimer { get; set; }

        private int diff;
        
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

                status.knockBack -= diff;
            }
        }
        
        public override void Run()
        {
            if (IsUse && false == isInvoke)
            {
                StartVFXRPC();
                isInvoke = true;

                diff = level.Current;
                
                status.knockBack += diff;
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }
    }
}
