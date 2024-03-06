using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Item.Looting;
using Manager;
using Photon;
using Script.Data;
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

        public static Action StageClearAction;

        #endregion

        #region Networked Variable

        private ChangeDetector _changeDetector;
        [Networked] [Capacity(3)] public NetworkArray<NetworkBool> IsStageUnload { get; }
        [Networked] public NetworkBool IsInit { get; set; }

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
        public List<NetworkSpawner> monsterSpawnerList = new List<NetworkSpawner>(); // 맵에 몬스터 스포너들
        public StatusValue<int> aliveMonsterCount = new StatusValue<int>(); // 한 맵에 최대 몇마리 살아있게 할 것인지
        public StatusValue<int> monsterKillCount = new StatusValue<int>(); // 몬스터 소멸 횟수

        #region Unity Event Function

        public void Awake()
        {
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
            stageGameObject.SetActive(false);
        }

        public virtual void Start()
        {
            aliveMonsterCount.isOverMax = true;
            monsterKillCount.isOverMax = true;
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (IsInit)
            {
                StageInit();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (isStageClear == false && isStageOver == false)
            {
                StageUpdate();
            }
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(IsStageUnload): // 스테이지 초기화 되면 스테이지를 부른 Scene을 Unload
                        if (stageLevelInfo.StageLevelType == StageLevelType.None)
                            continue;
                        int count = Runner.ActivePlayers.ToArray().Length;
                        foreach (var value in IsStageUnload)
                        {
                            if (value) --count;
                        }

                        if (count == 0)
                        {
                            NetworkManager.UnloadScene(sceneReference.ScenePath);
                        }
                        break;
                    case nameof(IsInit): // 스테이지 초기화 동기화를 위해 사용
                        if (IsInit)
                        {
                            StageInit();
                        }
                        break;
                }
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
                if (aliveMonsterCount.isMax)
                {
                    break;
                }

                aliveMonsterCount.Current += monsterSpawner.spawnCount.Max;
                monsterSpawner.SpawnSuccessAction += (obj) =>
                {
                    // obj.transform.SetParent(monsterParentTransform);

                    var monster = obj.GetComponent<MonsterBase>();
                    monster.DieAction += () =>
                    {
                        --aliveMonsterCount.Current;
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

        public virtual void StageInit()
        {
            var childEventSystem = stageGameObject.GetComponentInChildren<EventSystem>();
            var childCamera = stageGameObject.GetComponentInChildren<Camera>();
            if (childEventSystem != null)
            {
                Destroy(childEventSystem.gameObject);
            }
            if (childCamera != null)
            {
                Destroy(childCamera.gameObject);
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

            SetIsUnloadRPC(UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber, true);
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
            
            StageClearAction?.Invoke();
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

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsUnloadRPC(int clientNumber, NetworkBool value) => IsStageUnload.Set(clientNumber, value);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsInitRPC(bool value) => IsInit = value;

        #endregion
    }
}