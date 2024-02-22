using System;
using System.Collections.Generic;
using Fusion;
using Item.Looting;
using Manager;
using Photon;
using Script.Manager;
using Script.Monster;
using Script.Photon;
using Scripts.State.GameStatus;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Util.Map;

namespace GamePlay.StageLevel
{
    public class StageLevelBase : NetworkBehaviourEx
    {
        #region Static Variable

        public static Action<NetworkRunner> StageClearAction;

        #endregion
        
        public SceneReference sceneReference;

        [Header("스테이지 기본 정보")] 
        public StageLevelInfo stageLevelInfo;
        public bool isStageClear = false;
        public bool isStageOver = false;
        [HideInInspector] [Tooltip("보상 루팅 테이블")] public LootingTable lootingTable;

        [Header("맵 정보")]
        public GameObject stageGameObject;
        [SerializeField] private MapInfoMono _mapInfoMono;

        public MapInfo MapInfo
        {
            get => _mapInfoMono.info;
            set => _mapInfoMono.info = value;
        }

        [Header("몬스터 정보")] 
        public List<NetworkSpawner> monsterSpawnerList = new List<NetworkSpawner>();
        public StatusValue<int> monsterKillCount = new StatusValue<int>(); // 몬스터를 소멸 시킨 수

        public string goalInfo;
        public string awardInfo;
        public string difficultInfo;

        #region Unity Event Function

        public void Awake()
        {
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
            stageGameObject.SetActive(false);
            monsterKillCount.isOverMax = true;
        }

        private void Start()
        {
            stageLevelInfo.AliveMonsterCount.isOverMax = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (isStageClear == false && isStageOver == false)
            {
                StageUpdate();
            }
        }

        #endregion

        #region Defualt Function

        // 스테이지에 들어서면 정보에 맞춰 해당 스테이지를 셋팅 해준다.
        public void StageSetting()
        {
            StartMonsterSpawn();
        }

        public void StartMonsterSpawn()
        {
            foreach (var monsterSpawner in monsterSpawnerList)
            {
                if (stageLevelInfo.AliveMonsterCount.isMax)
                {
                    break;
                }

                stageLevelInfo.AliveMonsterCount.Current += monsterSpawner.spawnCount.Max;
                monsterSpawner.SpawnSuccessAction += (obj) =>
                {
                    // obj.transform.SetParent(monsterParentTransform);

                    var monster = obj.GetComponent<MonsterBase>();
                    monster.DieAction += () =>
                    {
                        --stageLevelInfo.AliveMonsterCount.Current;
                        --monsterSpawner.spawnCount.Current;
                        ++monsterKillCount.Current;
                    };
                };
                monsterSpawner.SpawnStart();
            }
        }

        public void StopMonsterSpawn()
        {
            foreach (var monsterSpawner in monsterSpawnerList)
            {
                monsterSpawner.SpawnStop();
            }
        }

        #endregion

        #region Stage Function

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public virtual void StageInitRPC() => StageInit();

        public virtual void StageInit()
        {
            if (stageGameObject.GetComponentInChildren<EventSystem>() != null)
            {
                Destroy(stageGameObject.GetComponentInChildren<EventSystem>().gameObject);
            }

            if (stageGameObject.GetComponentInChildren<Camera>() != null)
            {
                Destroy(stageGameObject.GetComponentInChildren<Camera>().gameObject);
            }

            Runner.MoveGameObjectToSameScene(gameObject, GameManager.Instance.gameObject);
            Runner.MoveGameObjectToSameScene(stageGameObject, GameManager.Instance.gameObject);

            gameObject.transform.position = MapInfo.pivot;
            stageGameObject.transform.position = MapInfo.pivot;
            stageGameObject.SetActive(true);

            if (LootingSystem.Instance.stageLootingItemDictionary.TryGetValue((int)stageLevelInfo.StageLevelType, out var lootingItems))
            {
                lootingTable.CalLootingItem(lootingItems);
            }
            
            DebugManager.Log($"스테이지 초기화 {stageLevelInfo.title}");
        }

        public virtual void StageUpdate()
        {
            if (GameManager.Instance.AlivePlayerCount <= 0)
            {
                StageOver();
            }
        }

        public virtual void StageClear()
        {
            if (isStageClear)
            {
                return;
            }
            
            isStageClear = true;
            
            lootingTable.SpawnDropItem();
            DebugManager.ToDo("임시적으로 모든 아이템을 드랍하게 함");
            
            StageClearAction?.Invoke(Runner);
            DebugManager.Log("스테이지 클리어\n" +
                             $"스테이지 모드 :{stageLevelInfo.StageLevelType}");
        }

        public virtual void StageOver()
        {
            if (isStageOver)
            {
                return;
            }
            
            isStageOver = true;
        }

        #endregion
    }
}