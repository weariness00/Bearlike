using System.Collections.Generic;
using Fusion;
using Script.Data;
using Script.Util;
using UnityEngine.SceneManagement;

namespace Photon
{
    public class NetworkMatchManager : NetworkBehaviour
    {
        public static NetworkMatchManager Instance;
        public MatchRoomUserUI roomUserUI;

        public List<NetworkPrefabRef> PlayerPrefabRefs;

        public void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void DataUpdate()
        {
            var userData = FindObjectOfType<UserData>();
            
            var items = NetworkUtil.DictionaryItems(userData.UserDictionary);
            roomUserUI.UpdateData(items);
        }

        public void NextPlayerPrefab()
        {
            var userData = FindObjectOfType<UserData>();

            userData.ChangePlayerRef(Runner.LocalPlayer, PlayerPrefabRefs[0]);
        }
        
        void GameStart()
        {
            gameObject.SetActive(false);
            
            var sceneRef = SceneRef.FromIndex((int)SceneType.Game);
            LoadSceneParameters lp = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Single,
                localPhysicsMode = LocalPhysicsMode.Physics3D,
            };
            
            var networkSceneAsyncOp = Runner.LoadScene(sceneRef, lp, true);
        }
    }
}

