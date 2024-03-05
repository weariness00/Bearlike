using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GamePlay.StageLevel;
using Photon;
using Script.Data;
using Script.GamePlay;
using Script.Manager;
using Scripts.State.GameStatus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util.Map;
using Random = UnityEngine.Random;

namespace Manager
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        #region Network Variable
        
        [Networked] public float PlayTimer { get; set; }
        [Networked] public float AlivePlayerCount { get; set; }
        
        #endregion

        [SerializeField]private SpawnPlace _spawnPlace = new SpawnPlace();

        private MapGenerate _mapGenerate = new MapGenerate();

        [Header("스테이지")]
        public StageLevelBase defaultStage;
        public List<StageLevelBase> stageList = new List<StageLevelBase>();
        public StatusValue<int> stageCount = new StatusValue<int>();// 현재 몇번째 스테이지 인지

        #region Unity Event Function
        protected override void Awake()
        {
            base.Awake();
            _spawnPlace.Initialize();
        }

        public override void Spawned()
        {
            if (Runner.IsServer == false)
            {
                return;
            }
            
            Init();
            UserInit();
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer == false)
            {
                return;
            }
            
            PlayTimer += Runner.DeltaTime;
        }
        #endregion

        #region Inisialize

        void Init()
        {
            // defaultStage.MapInfo = _mapGenerate.FindEmptySpace(defaultStage.MapInfo);
            defaultStage.SetIsInitRPC(true);
            _mapGenerate.AddMap(defaultStage.MapInfo);
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
        
        public StageLevelBase GetRandomStage() => GetStageIndex(Random.Range(0, stageList.Count));

        public StageLevelBase GetStageIndex(int index)
        {
            return stageList[index];
        }

        public async void SetStage(StageLevelBase stage)
        {
            if (stage == null)
            {
                return;
            }

            await NetworkManager.LoadScene(stage.sceneReference.ScenePath,LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            async void OnSceneLoadDoneAction()
            {
                foreach (var stageLevelBase in FindObjectsOfType<StageLevelBase>())
                {
                    // 이미 활성화된 스테이지
                    if (stageLevelBase.stageGameObject.activeSelf)
                    {
                        continue;
                    }

                    if (stage.stageLevelInfo.StageLevelType == stageLevelBase.stageLevelInfo.StageLevelType)
                    {
                        stageLevelBase.MapInfo = await _mapGenerate.FindEmptySpaceSync(stage.MapInfo, defaultStage.MapInfo);
                        stageLevelBase.SetIsInitRPC(true);

                        _mapGenerate.AddMap(stageLevelBase.MapInfo);
                        stageLevelBase.StageSetting();

                        DebugManager.Log($"씬 생성 후 초기화 완료 {stage.sceneReference}");
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

