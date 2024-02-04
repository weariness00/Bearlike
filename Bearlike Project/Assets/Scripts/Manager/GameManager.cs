using System.Collections.Generic;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GamePlay.StageLevel;
using Photon;
using Script.Data;
using Script.GamePlay;
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
            base.Spawned();
            Init();
            UserInit();
        }

        public override void FixedUpdateNetwork()
        {
            PlayTimer += Runner.DeltaTime;
        }
        #endregion

        void Init()
        {
            defaultStage.MapInfo = _mapGenerate.FindEmptySpace(defaultStage.MapInfo);
            defaultStage.StageInit();
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
            foreach (var stageLevelBase in FindObjectsOfType<StageLevelBase>())
            {
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

