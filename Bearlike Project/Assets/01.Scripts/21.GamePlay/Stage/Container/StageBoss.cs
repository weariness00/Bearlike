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
            base.StageClear();

            GameManager.Instance.GameClear();
            nextStagePortal.otherPortal = GameManager.Instance.gameClearPortal;
            if(nextStagePortal.portalVFXList.Count >= 5) nextStagePortal.portalVFXList[0].gameObject.SetActive(true);
            nextStagePortal.IsConnect = true; // 현재 진행중인 스테이지의 포탙 개방
        }
    }
}