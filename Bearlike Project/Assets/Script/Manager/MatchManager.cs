using Fusion;
using Script.Data;
using Script.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Manager
{
    public class MatchManager : NetworkBehaviour
    {
        public MatchRoomUserUI roomUserUI;
        public UserData userData;

        public NetworkPrefabRef pf;

        private NetworkRunner _runner;
        

        private void Start()
        {
            _runner = FindObjectOfType<NetworkRunner>();
        }

        public void DataUpdate()
        {
            var items = NetworkUtil.DictionaryItems(userData.UserDictionary);
            roomUserUI.UpdateData(items);
        }
        
        async void GameStart()
        {
            gameObject.SetActive(false);
            
            var sceneRef = SceneRef.FromIndex((int)SceneType.Game);
            LoadSceneParameters lp = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Single,
                localPhysicsMode = LocalPhysicsMode.Physics3D,
            };

            _runner.LoadScene(sceneRef, lp);
        }
    }
}

