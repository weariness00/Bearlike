using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;

namespace GamePlay.StageLevel.Container
{
    public class StageSurvive : StageLevelBase
    {
        [Header("생존 스테이지 정보")]
        public StatusValue<float> timeLimit = new StatusValue<float>();

        public override void StageInit()
        {
            base.StageInit();
        }

        public override void StageUpdate()
        {
            base.StageUpdate();
            timeLimit.Current += Runner.DeltaTime;
            if (timeLimit.isMax)
            {
                StageClear();
            }
        }

        public override void StageClear()
        {
            if (isStageClear)
            {
                return;
            }
            base.StageClear();
        }

        public override void StageOver()
        {
            if (isStageOver)
            {
                return;
            }
            base.StageOver();
        }
    }
}

