namespace GamePlay.Stage.Container
{
    public class StageDestroy : StageBase
    {
        // public StatusValue<float> destroyTimeLimit = new StatusValue<float>();

        public override void StageUpdate()
        {
            base.StageUpdate();
            if (monsterKillCount.isMax)
            {
                StageClear();
            }
            
            // 실패 조건은 모든 플레이어가 죽을 경우
        }
        
    }
}

