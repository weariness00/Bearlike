using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Fusion;
using GamePlay.StageLevel;
using Item.Looting;
using Manager;
using Monster;
using Photon;
using Script.Photon;
using Status;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Stage
{
    [RequireComponent(typeof(LootingTable))]
    public class StageBase : NetworkBehaviourEx
    {
        #region Static Variable

        public static Action StageInitAction;
        public static Action StageClearAction;
        
        // Info Data 캐싱
        private static readonly Dictionary<int, StageJsonData> InfoDataCash = new Dictionary<int, StageJsonData>();
        public static void AddInfoData(int id, StageJsonData data) => InfoDataCash.TryAdd(id, data);
        public static StageJsonData GetInfoData(int id) => InfoDataCash.TryGetValue(id, out var data) ? data : new StageJsonData();
        public static void ClearInfosData() => InfoDataCash.Clear();
        
        // Looting Data 캐싱
        private static readonly Dictionary<int, LootingJsonData> LootingDataChasing = new Dictionary<int, LootingJsonData>();
        public static void AddLootingData(int id, LootingJsonData data) => LootingDataChasing.TryAdd(id, data);
        public static LootingJsonData GetLootingData(int id) => LootingDataChasing.TryGetValue(id, out var data) ? data : new LootingJsonData();
        public static void ClearLootingData() => LootingDataChasing.Clear();

        #endregion

        #region Networked Variable

        private ChangeDetector _changeDetector;
        [Networked] [Capacity(3)] public NetworkArray<NetworkBool> IsStageUnload { get; }
        [Networked] public NetworkBool IsInit { get; set; }
        [Networked] public NetworkBool IsStageStart { get; set; }
        
        #endregion

        public SceneReference sceneReference;

        [Header("스테이지 기본 정보")] 
        public StageData stageData;
        public bool isStageClear = false;
        public bool isStageOver = false;

        public StageInfo StageInfo => stageData.info;

        [HideInInspector] [Tooltip("보상 루팅 테이블")]
        public LootingTable lootingTable;

        [Header("맵 정보")] 
        public GameObject stageGameObject;

        public GameObject destructObject; // 스테이지 클리어시 붕괴가능하게 할 객체
        
        public Portal prevStagePortal;
        public Portal nextStagePortal;

        [Header("몬스터 정보")] public List<NetworkSpawner> monsterSpawnerList = new List<NetworkSpawner>(); // 맵에 몬스터 스포너들
        public StatusValue<int> aliveMonsterCount = new StatusValue<int>(); // 한 맵에 최대 몇마리 살아있게 할 것인지
        public StatusValue<int> monsterKillCount = new StatusValue<int>(); // 몬스터 소멸 횟수

        #region Unity Event Function

        public void Awake()
        {
            lootingTable = GetComponent<LootingTable>();
            stageGameObject.SetActive(false);
        }

        public virtual void Start()
        {
            aliveMonsterCount.isOverMax = true;
            monsterKillCount.isOverMax = true;
            
            StageInfo.SetJsonData(GetInfoData(StageInfo.id));
            lootingTable.CalLootingItem(GetLootingData(StageInfo.id).LootingItems);
        }

        public void OnTriggerEnter(Collider other)
        {
            // 플레이어가 스테이지에 입장하면 스테이지 시작
            // 한번 시작하고 다른 플레이어의 충돌을 체크하지 않기 위해 본래의 컴포넌트를 비활성화
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                SetIsStartRPC(true);
                Destroy(GetComponent<Collider>());
            }
        }

        public void OnApplicationQuit()
        {
            StageInitAction = null;
            StageClearAction = null;
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
            if (IsStageStart &&isStageClear == false && isStageOver == false)
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
                        if (stageData.info.stageType == StageType.None)
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
                    case nameof(IsStageStart):
                        if (IsStageStart)
                        {
                            StageStart();
                        }
                        break;
                }
            }
        }

        #endregion

        #region Defualt Function

        // 스테이지에 들어서면 정보에 맞춰 해당 스테이지를 셋팅 해준다.
        public void StartMonsterSpawn()
        {
            foreach (var monsterSpawner in monsterSpawnerList)
            {
                if(monsterSpawner == null){ continue;}
                if (aliveMonsterCount.isMax)
                {
                    break;
                }

                SetAliveMonsterCountRPC(StatusValueType.Current, monsterSpawner.spawnCount.Max);
                monsterSpawner.SpawnSuccessAction += (obj) =>
                {
                    // obj.transform.SetParent(monsterParentTransform);

                    var monster = obj.GetComponent<MonsterBase>();
                    monster.DieAction += () =>
                    {
                        SetAliveMonsterCountRPC(StatusValueType.Current,  --aliveMonsterCount.Current);
                        monsterSpawner.SetSpawnCountRPC(StatusValueType.Current, --monsterSpawner.spawnCount.Current);
                        SetMonsterKillCountRPC(StatusValueType.Current, ++monsterKillCount.Current);
                    };
                };
                monsterSpawner.SpawnStartRPC();
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
            
            var pos = new Vector3(0,(FindObjectsOfType<StageBase>().Length - 1) * 100,0);
            transform.position = pos;
            stageGameObject.transform.position = pos;
            
            stageGameObject.SetActive(true);
            
            // 포탈 연결
            if (GameManager.Instance.currentStage != null)
            {
                var portal = GameManager.Instance.currentStage.nextStagePortal;
                prevStagePortal.SetPortal(portal);
                portal.IsConnect = true; // 현재 진행중인 스테이지의 포탙 개방
            }
            GameManager.Instance.currentStage = this;
            
            SetIsUnloadRPC(UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber, true);

            StageInitAction?.Invoke();
            
            DebugManager.Log($"스테이지 초기화 {stageData.info.title}");
        }

        public virtual void StageStart()
        {
            DebugManager.Log($"스테이지 시작\n" +
                             $"스테이지 모드 :{stageData.info.title}");
            
            StartMonsterSpawn();
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

            GameManager.Instance.stageCount.Current++;
            StopMonsterSpawn();

            prevStagePortal.IsConnect = true;
            isStageClear = true;

            lootingTable.SpawnDropItem();
            DebugManager.ToDo("임시적으로 모든 아이템을 드랍하게 함");

            if (destructObject != null) destructObject.tag = "Destruction";

            StageClearAction?.Invoke();
            DebugManager.Log("스테이지 클리어\n" +
                             $"스테이지 모드 :{stageData.info.stageType}");
        }

        public virtual void StageOver()
        {
            if (isStageOver)
            {
                return;
            }
            
            DebugManager.Log($"스테이지 실패\n" +
                             $"스테이지 모드 :{stageData.info.title}");
            
            StopMonsterSpawn();
            isStageOver = true;
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsUnloadRPC(int clientNumber, NetworkBool value) => IsStageUnload.Set(clientNumber, value);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsInitRPC(bool value) => IsInit = value;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsStartRPC(NetworkBool value) => IsStageStart = value;

        [Rpc(RpcSources.All,RpcTargets.All)]
        public void SetAliveMonsterCountRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Min:
                    aliveMonsterCount.Min = value;
                    break;
                case StatusValueType.Current:
                    aliveMonsterCount.Current = value;
                    break;
                case StatusValueType.Max :
                    aliveMonsterCount.Max = value;
                    break;
            }
        }
        [Rpc(RpcSources.All,RpcTargets.All)]
        public void SetMonsterKillCountRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Min:
                     monsterKillCount.Min = value;
                    break;
                case StatusValueType.Current:
                    monsterKillCount.Current = value;
                    break;
                case StatusValueType.Max :
                    monsterKillCount.Max = value;
                    break;
            }
        }
        
        #endregion
    }
}