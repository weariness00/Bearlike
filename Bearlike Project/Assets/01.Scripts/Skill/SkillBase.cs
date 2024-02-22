using Fusion;
using Scripts.State.GameStatus;

namespace Skill
{
    public abstract class SkillBase
    {
        [Networked] public StatusValue<float> Duration
        {
            get => _duration;
            set => _duration = value;
        }
        
        [Networked] public StatusValue<float> CoolTime
        {
            get => _coolTime;
            set => _coolTime = value;
        }

        private StatusValue<float> _duration = new StatusValue<float>();
        private StatusValue<float> _coolTime = new StatusValue<float>();

        public abstract void MainLoop();
        public abstract void Run();
    }
}