using System.Collections.Generic;
using System.Linq;
using Monster;
using Script.Photon;
using Status;

namespace GamePlay.Stage.Container
{
    public class StageDestroy : StageBase
    {
        // public StatusValue<float> destroyTimeLimit = new StatusValue<float>();

        public List<NetworkSpawner> waveList = new List<NetworkSpawner>();
        public List<int> waveInvokeConditionList = new List<int>(); // 킬 수가 채월질때마다 다음 웨이브를 부른다.

        private int _currentWaveCount = 0;
        private int WaveListLength => waveList.Count;

        public override void StageUpdate()
        {
            base.StageUpdate();
            if (monsterKillCount.isMax)
            {
                StageClear();
                return;
            }

            if (_currentWaveCount < WaveListLength && 
                monsterKillCount.Current > waveInvokeConditionList[_currentWaveCount])
            {
                var monsterSpawner = waveList[_currentWaveCount];
            
                SetAliveMonsterCountRPC(StatusValueType.Current, monsterSpawner.spawnCount.Max);
                monsterSpawner.SpawnSuccessAction += (obj) =>
                {
                    var monster = obj.GetComponent<MonsterBase>();
                    monster.DieAction += () =>
                    {
                        SetAliveMonsterCountRPC(StatusValueType.Current,  --aliveMonsterCount.Current);
                        monsterSpawner.SetSpawnCountRPC(StatusValueType.Current, --monsterSpawner.spawnCount.Current);
                        SetMonsterKillCountRPC(StatusValueType.Current, ++monsterKillCount.Current);
                    };
                };
                monsterSpawner.SpawnStartRPC();
                
                ++_currentWaveCount;
            }
            
            // 실패 조건은 모든 플레이어가 죽을 경우
        }

        public override void StageClear()
        {
            if(isStageClear) return;

            foreach (var monsterSpawner in waveList)
                monsterSpawner.SpawnStop();
            
            base.StageClear();
        }
    }
}

