using System.Collections.Generic;
using Data;
using Fusion;
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
        
        async void GameStart()
        {
            gameObject.SetActive(false);
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            await NetworkManager.LoadScene(SceneType.Game, LoadSceneMode.Single, LocalPhysicsMode.Physics3D);
        }
    }
}

