using Status;

namespace Skill.Container
{
    /// <summary>
    /// 샷건의 산탄 총알들이 강력해져서 몬스터들은 뒤로 밀도록 개조하는 스킬
    /// 쿨타임 - 30초, 지속 시간 : 10초 (패시브로 할까 생각중) 
    /// </summary>
    public class KnockbackShot : SkillBase
    {
        #region Value

        public StatusValue<float> duration = new StatusValue<float>();
        private const float AttackValue = 0.5f;
        private const float AttackSpeedValue = 0.2f;
        private const float CoolTime = 30.0f;
        private const float DurationTime = 10.0f;

        #endregion
        
        public override void Awake()
        {
            base.Awake();
            
        }

        public override void MainLoop()
        {
        }

        public override void Run()
        {
            
        }

    }
}
