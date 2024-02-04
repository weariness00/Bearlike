using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GamePlay.StageLevel;
using Photon;
using Script.Data;
using Script.GamePlay;
using Script.Manager;
using Script.Photon;
using Script.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Util.Map;

namespace Manager
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        [SerializeField]private SpawnPlace _spawnPlace = new SpawnPlace();

        private MapGenerate _mapGenerate = new MapGenerate();
        [Header("스테이지")]
        public StageLevelBase defaultStage;
        public List<StageLevelBase> stageList = new List<StageLevelBase>();

        #region Data Property

        [Networked] public float PlayTimer { get; set; }

        #endregion

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
            
            base.Spawned();
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

        void Init()
        {
            defaultStage.MapInfo = _mapGenerate.FindEmptySpace(defaultStage.MapInfo);
            defaultStage.StageInitRPC();
            _mapGenerate.AddMap(defaultStage.MapInfo);
        }
        
        async void UserInit()
        {   
            await UserData.Instance.SpawnPlayers();
            if (Runner != null && Runner.IsServer)
            {
                foreach (var (key, user) in UserData.Instance.UserDictionary)
                {
                    // var playerController = Runner.FindObject(user.NetworkId).GetComponent<PlayerController>();
                    UserData.SetTeleportPosition(key, _spawnPlace.GetRandomSpot().position);
                }
            }
        }

        #region Stage Logic Function

        public StageLevelBase GetRandomStage()
        {
            return stageList.Count == 0 ? null : stageList[Random.Range(0, stageList.Count)];
        }

        public async void SetStage(StageLevelBase stage)
        {
            if (stage == null)
            {
                return;
            }

            await NetworkManager.LoadScene(stage.sceneReference.ScenePath,LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            
            DebugManager.ToDo("임시 방편으로 0.1초 기다린뒤 초기화를 진행함\n" +
                              "씬이 모든 클라이언트에서 로드 된 것을 알 수 있게하는 동기화 기법을 사용해야됨");
            await Task.Delay(100);
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
                    stageLevelBase.StageInitRPC();
                    
                    // NetworkManager.UnloadScene(stage.sceneReference.ScenePath);
                    
                    _mapGenerate.AddMap(stageLevelBase.MapInfo);
                    break;
                }
            }
        }

        public void SetStage(int index) => SetStage(stageList.Count < index ? null : stageList[index]);
        #endregion
    }
}

