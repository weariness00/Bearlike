using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Manager;
using Script.Manager;
using Script.Monster;
using Script.Photon;
using Scripts.State.GameStatus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Util.Map;

namespace GamePlay.StageLevel
{
    public class StageLevelBase : NetworkBehaviour
    {
        public static Action stageClearAction = null; // 모든 스테이지에서 공통적으로 쓰이기 떄문에 전역으로 처리함
        
        public SceneReference sceneReference;
        private Action _stageUpdateAction = null;
        
        #region Stage Info Variable

        [Header("스테이지 정보")]
        public StageLevelInfo stageLevelInfo;
        public bool isStageClear = false;

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
        public Transform monsterParentTransform = null;

        #endregion
        
        #region Destroy Variable

        public StatusValue<float> destroyTimeLimit = new StatusValue<float>();
        public StatusValue<int> monsterKillCount = new StatusValue<int>();

        #endregion
        
        public string goalInfo; 
        public string awardInfo;
        public string difficultInfo;

        #region Unity Event Function

        private void Awake()
        {
            stageGameObject.SetActive(false);
            StageMemoryOptimize();
            switch (stageLevelInfo.StageLevelType)
            {
                case StageLevelType.Destroy:
                    DestroyInit();
                    _stageUpdateAction = DestroyUpdate;
                    break;
            }
        }

        private void Start()
        {
            stageLevelInfo.AliveMonsterCount.isOverMax = true;
            if (monsterParentTransform == null) { monsterParentTransform = gameObject.transform; }

            stageClearAction += StopMonsterSpawn;
        }

        public override void FixedUpdateNetwork()
        {
            _stageUpdateAction?.Invoke();
        }

        #endregion

        #region Defualt Function

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void StageInitRPC() => StageInit();
        public void StageInit()
        {
           if(stageGameObject.GetComponentInChildren<EventSystem>() != null){Destroy(stageGameObject.GetComponentInChildren<EventSystem>().gameObject);}
            if(stageGameObject.GetComponentInChildren<Camera>() != null){Destroy(stageGameObject.GetComponentInChildren<Camera>().gameObject);}
            
            Runner.MoveGameObjectToSameScene(gameObject, GameManager.Instance.gameObject);
            Runner.MoveGameObjectToSameScene(stageGameObject, GameManager.Instance.gameObject);
            
            gameObject.transform.position = MapInfo.pivot;
            stageGameObject.transform.position = MapInfo.pivot;
            stageGameObject.SetActive(true);
        }

        // 스테이지 타입에 맞지 않는 멥버 변수 메모리 해제
        void StageMemoryOptimize()
        {
            if (stageLevelInfo.StageLevelType != StageLevelType.Destroy)
            {
                destroyTimeLimit = null;
                monsterKillCount = null;
            }
        }
        
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
                    obj.transform.parent = monsterParentTransform;
                    
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

        void StageClear()
        {
            
        }
        
        #endregion

        #region Destroy Function

        void DestroyInit()
        {
            monsterKillCount.isOverMax = true;
        }

        void DestroyUpdate()
        {
            destroyTimeLimit.Current += Runner.DeltaTime;
            if (monsterKillCount.isMax)
            {
                DebugManager.Log("스테이지 클리어\n" +
                                 $"스테이지 모드 :{stageLevelInfo.StageLevelType}");
                isStageClear = true;
                _stageUpdateAction -= DestroyUpdate;
                DestroyClear();
                
                stageClearAction?.Invoke();
            }
            else if (destroyTimeLimit.isMax)
            {
                DestroyOver();
                _stageUpdateAction = null;
                return;
            }
        }

        void DestroyClear()
        {
            
        }

        void DestroyOver()
        {
            
        }

        #endregion
    }
}