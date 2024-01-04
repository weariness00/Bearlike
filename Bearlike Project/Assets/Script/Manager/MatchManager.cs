using System.Collections.Generic;
using System.Linq;
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

        public List<NetworkPrefabRef> PlayerPrefabRefs;
        
        public void DataUpdate()
        {
            var userData = FindObjectOfType<UserData>();
            
            var items = NetworkUtil.DictionaryItems(userData.UserDictionary);
            roomUserUI.UpdateData(items);
        }

        void NextPlayerPrefab()
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

            Runner.LoadScene(sceneRef, lp);
        }
    }
}

