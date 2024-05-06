using System.Collections.Generic;
using System.Linq;
using Data;
using Fusion;
using GamePlay.Stage;
using GamePlay.StageLevel;
using Loading;
using Manager;
using Photon;
using Script.Data;
using Script.GamePlay;
using Status;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace GamePlay
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.GameSceneStart)]
    public class GameManager : NetworkSingleton<GameManager>
    {
        #region Network Variable
        
        [Networked] public float PlayTimer { get; set; }
        [Networked] public float AlivePlayerCount { get; set; }
        
        #endregion

        public bool isControl = true; // 게임중에 플레이어나 다른 무언가들의 컨트롤을 가능하게 할지
        public bool isGameClear; // 게임을 완전 클리어 했을때
        public bool isGameOver;
        public Portal gameClearPortal;
        
        [SerializeField]private SpawnPlace _spawnPlace = new SpawnPlace();

        [Header("스테이지")]
        public StageBase defaultStage;
        [Tooltip("보스 스테이지를 마지막 인덱스에 넣어줘야함")]public List<StageData> stageList = new List<StageData>();
        public StageBase currentStage;
        public StatusValue<int> stageCount = new StatusValue<int>();// 현재 몇번째 스테이지 인지

        public SceneReference gmModeScene;
        public SceneReference gameResultScene;
        public SceneReference loadingScene;
        
        #region Unity Event Function
        protected override void Awake()
        {
            base.Awake();
            _spawnPlace.Initialize();
            NetworkManager.LoadScene(loadingScene, LoadSceneMode.Additive);
        }

        public override void Spawned()
        {
            if (NetworkManager.Instance.isTest)
            {
                NetworkManager.LoadScene(gmModeScene, LoadSceneMode.Additive);
            }
            
            if (Runner.IsServer == false)
            {
                return;
            }
            
            Init();
            UserInit();
        }

        public override void FixedUpdateNetwork()
        {
            if (isGameClear)
            {
                DebugManager.ToDo("게임을 완전 클리어하면 로비로 돌아가는 포탈 생성해주기");
                gameClearPortal.gameObject.SetActive(true);
                gameClearPortal.InteractEnterAction = async (obj) =>
                {
                    await NetworkManager.LoadScene(SceneType.Lobby);
                };
            }
            
            PlayTimer += Runner.DeltaTime;
        }
        #endregion

        #region Inisialize

        void Init()
        {
            defaultStage.SetIsInitRPC(true);
            StageBase.StageOverAction += () => { NetworkManager.LoadScene(gameResultScene, LoadSceneMode.Additive); };
        }
        
        async void UserInit()
        {   
            await UserData.Instance.SpawnPlayers();
            if (Runner != null && Runner.IsServer)
            {
                foreach (var (key, user) in UserData.Instance.UserDictionary)
                {
                    UserData.SetTeleportPosition(key, _spawnPlace.GetRandomSpot().position);
                    AlivePlayerCount++;
                }
            }
        }

        #endregion
        
        #region Stage Logic Function

        public StageData GetRandomStage() => GetStageIndex(Random.Range(0, stageList.Count));
        public StageData GetBossStage() => stageList.Last();
        public StageData GetStageIndex(int index) => stageList[index];

        public async void SetStage(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }

            await NetworkManager.LoadScene(stageData.sceneReference.ScenePath,LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            async void OnSceneLoadDoneAction()
            {
                 foreach (var stageLevelBase in FindObjectsOfType<StageBase>())
                 {
                    // 이미 활성화된 스테이지
                    if (stageLevelBase.stageGameObject.activeSelf)
                    {
                        continue;
                    }

                    if (stageData.info.stageType == stageLevelBase.StageInfo.stageType)
                    {
                        stageLevelBase.SetIsInitRPC(true);
                        
                        DebugManager.Log($"씬 생성 후 초기화 완료 {stageData.sceneReference}");
                        break;
                    }
                 }
            }
            NetworkManager.SceneLoadDoneAction += OnSceneLoadDoneAction;
        }

        public void SetStage(int index) => SetStage(stageList.Count < index ? null : stageList[index]);
        
        #endregion
    }
}

