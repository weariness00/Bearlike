﻿using System.Collections.Generic;
using Data;
using Fusion;
using Script.Data;
using Script.Util;
using UnityEngine.SceneManagement;

namespace Photon
{
    public class NetworkMatchManager : NetworkBehaviour
    {
        public MatchRoomUserUI roomUserUI;

        public List<NetworkPrefabRef> PlayerPrefabRefs;

        public void NextPlayerPrefab()
        {
            var userData = FindObjectOfType<UserData>();

            userData.ChangePlayerRef(Runner.LocalPlayer, PlayerPrefabRefs[0]);
        }
        
        void GameStart()
        {
            gameObject.SetActive(false);
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            NetworkManager.LoadScene(SceneType.Game, LoadSceneMode.Single, LocalPhysicsMode.Physics3D);
        }
    }
}
