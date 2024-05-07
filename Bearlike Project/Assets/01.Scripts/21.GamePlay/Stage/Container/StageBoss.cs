using System.Collections.Generic;
using GamePlay.StageLevel;
using Manager;
using Monster;
using Photon;
using Script.Photon;
using Status;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Stage.Container
{
    public class StageBoss : StageBase
    {
        [Header("보스 정보")]
        public List<NetworkSpawner> bossSpawnerList;
        public StatusValue<int> bossMonsterCount = new StatusValue<int>(); 

        public override void StageStart()
        {
            base.StageStart();
            DebugManager.ToDo("나중에 보스 여러마리 소환하고 싶으면 여기 로직 변경해야된다.");
            foreach (var bossSpawner in bossSpawnerList)
            {
                // 일단 보스는 한마리만 소환하도록 함
                bossSpawner.SpawnSuccessAction += (obj) =>
                {
                    bossSpawner.SpawnStop();
                    var monster = obj.GetComponent<MonsterBase>();
                    ++bossMonsterCount.Max;
                    ++bossMonsterCount.Current;
                    
                    monster.DieAction += () =>
                    {
                        --bossMonsterCount.Current;
                    };
                };
                bossSpawner.SpawnStartRPC();
            }
        }

        public override void StageUpdate()
        {
            base.StageUpdate();
            if (bossMonsterCount.isMin)
            {
                isStageClear = true;
            }
        }

        public override void StageClear()
        {
            if (isStageClear)
                return;
            
            if (destructObject != null) destructObject.tag = "Destruction";
            
            nextStagePortal.InteractKeyDownAction = (obj) => NetworkManager.Runner.Shutdown();
            
            DebugManager.Log("스테이지 클리어\n" +
                             $"스테이지 모드 :{stageData.info.stageType}");
        }
    }
}