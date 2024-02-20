using Script.Manager;
using Scripts.State.GameStatus;

namespace GamePlay.StageLevel.Container
{
    public class StageDestroy : StageLevelBase
    {
        public StatusValue<float> destroyTimeLimit = new StatusValue<float>();
        
        public override void StageUpdate()
        {
            base.StageUpdate();
            destroyTimeLimit.Current += Runner.DeltaTime;
            if (monsterKillCount.isMax)
            {
                StageClear();
            }
            else if (destroyTimeLimit.isMax)
            {
                StageOver();
            }
        }
    }
}

