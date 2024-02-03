using System.Collections.Generic;
using Fusion;
using GamePlay.StageLevel;
using Photon;
using Script.Data;
using Script.GamePlay;
using Script.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Util.Map;

namespace Manager
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        private UserData _userData;
        [SerializeField]private SpawnPlace _spawnPlace = new SpawnPlace();

        private MapGenerate _mapGenerate = new MapGenerate();
        [Header("스테이지")]
        public StageLevelBase defaultStage;
        public List<StageLevelBase> stageList = new List<StageLevelBase>();

        #region Data Property

        public float playTimer = 0f; 

        #endregion

        #region Unity Event Function
        protected override void Awake()
        {
            base.Awake();
            _spawnPlace.Initialize();
        }

        private void Start()
        {
            _userData = FindObjectOfType<UserData>();
        
            UserInit();
        
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void Spawned()
        {
            base.Spawned();
            Init();
        }

        public override void FixedUpdateNetwork()
        {
            playTimer += Runner.DeltaTime;
        }
        #endregion

        async void Init()
        {
            defaultStage.MapInfo = await _mapGenerate.FindEmptySpace(defaultStage.MapInfo);
            defaultStage.StageInit();
            _mapGenerate.AddMap(defaultStage.MapInfo);
        }
        
        async void UserInit()
        {   
            await _userData.SpawnPlayers();
            if (Runner != null && Runner.IsServer)
            {
                foreach (var (key, user) in _userData.UserDictionary)
                {
                    var playerObject = Runner.FindObject(user.NetworkId);
                    playerObject.transform.position = _spawnPlace.GetRandomSpot().position;
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

            await NetworkManager.LoadScene(stage.sceneReference.ScenePath, LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            foreach (var stageLevelBase in FindObjectsOfType<StageLevelBase>())
            {
                if (stage.stageLevelInfo.StageLevelType == stageLevelBase.stageLevelInfo.StageLevelType)
                {
                    stageLevelBase.MapInfo = await _mapGenerate.FindEmptySpace(stage.MapInfo, defaultStage.MapInfo);
                    stageLevelBase.StageInit();
                    
                    NetworkManager.UnloadScene(stage.sceneReference.ScenePath);
                    
                    _mapGenerate.AddMap(stageLevelBase.MapInfo);
                    break;
                }
            }
        }

        public void SetStage(int index) => SetStage(stageList.Count < index ? null : stageList[index]);
        #endregion
    }
}

