using System.Collections.Generic;
using Fusion;
using GamePlay.StageLevel;
using Photon;
using Script.Data;
using Script.GamePlay;
using UnityEngine;

namespace Manager
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        private UserData _userData;
        [SerializeField]private SpawnPlace _spawnPlace = new SpawnPlace();

        public List<StageLevelBase> sceneList = new List<StageLevelBase>();

        #region Data Property

        public float playTimer = 0f; 

        #endregion
    
        private void Awake()
        {
            _spawnPlace.Initialize();
        }

        private void Start()
        {
            _userData = FindObjectOfType<UserData>();
        
            UserInit();
        
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void FixedUpdateNetwork()
        {
            playTimer += Runner.DeltaTime;
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
    }
}

