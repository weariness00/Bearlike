using GamePlay.StageLevel;
using Monster;
using Script.Photon;

namespace GamePlay.Stage.Container
{
    public class StageBoss : StageLevelBase
    {
        public NetworkSpawner bossSpawner;

        public override void StageStart()
        {
            base.StageStart();
            bossSpawner.SpawnSuccessAction += (obj) =>
            {
                var monster = obj.GetComponent<MonsterBase>();
                monster.DieAction += () =>
                {
                    isStageClear = true;
                };
            };
            bossSpawner.SpawnStart();
        }

        public override void StageClear()
        {
            base.StageClear();
            
        }
    }
}