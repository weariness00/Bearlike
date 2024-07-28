using System.Collections.Generic;
using Data;
using Manager;
using Monster;
using Photon.MeshDestruct;
using Player;
using Script.Photon;
using Status;
using UnityEngine;
using UnityEngine.Playables;
using Util.UnityEventComponent;

namespace GamePlay.Stage.Container
{
    public class StageBoss : StageBase
    {
        [Header("보스 정보")]
        public List<NetworkSpawner> bossSpawnerList;
        public StatusValue<int> bossMonsterCount = new StatusValue<int>();

        [Header("시네마틱")] 
        [SerializeField] private Collider cinematicCollider; // 이 콜라이더와 충돌하면 시네마틱 실행
        [SerializeField] private GameObject rescueCinematic;
        [SerializeField] private PlayableDirector cinematicPlayableDirector;
        [SerializeField] private List<Transform> playerTPTransformList = new List<Transform>();
        private bool isStartCinematic = false;

        public override void Spawned()
        {
            base.Spawned();
            
            cinematicCollider.gameObject.AddOnTriggerEnter((other) =>
            {
                if (isStartCinematic) return;
                isStartCinematic = true;

                var sliceObjects = FindObjectsOfType<NetworkMeshSliceObject>();
                foreach (var sliceObject in sliceObjects)
                    sliceObject.gameObject.SetActive(false);

                var players = FindObjectsOfType<PlayerController>();
                foreach (var player in players)
                    player.gameObject.SetActive(false);
                
                rescueCinematic.SetActive(true);
                cinematicPlayableDirector.stopped += director =>
                {
                    rescueCinematic.SetActive(false);
                    
                    // 모든 클라에서 실행되어야함
                    GameManager.Instance.GameClear();
                    nextStagePortal.otherPortal = GameManager.Instance.gameClearPortal;
                    if(nextStagePortal.portalVFXList.Count >= 5) nextStagePortal.portalVFXList[0].gameObject.SetActive(true);
                    nextStagePortal.IsConnect = true; // 현재 진행중인 스테이지의 포탙 개방
                    
                    for (var i = 0; i < players.Length; i++)
                    {
                        var player = players[i];
                        player.gameObject.SetActive(true);
                        if(HasStateAuthority)
                            UserData.SetTeleportPosition(player.Object.InputAuthority, playerTPTransformList[i].position);
                    }
                };
                cinematicPlayableDirector.Play();
                
                Destroy(cinematicCollider);
            });
        }

        public override void StageInit()
        {
            base.StageInit();
            Runner.MoveGameObjectToSameScene(cinematicPlayableDirector.gameObject, GameManager.Instance.gameObject);
            cinematicPlayableDirector.transform.position = stageGameObject.transform.position;
        }

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
                StageClear();
            }
        }
    }
}