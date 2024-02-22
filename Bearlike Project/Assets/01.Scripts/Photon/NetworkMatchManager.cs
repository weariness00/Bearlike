using System.Collections.Generic;
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

        [Rpc(RpcSources.All,RpcTargets.All)]
        public void DataUpdateRPC() => DataUpdate();
        public void DataUpdate()
        {
            var items = NetworkUtil.DictionaryItems(UserData.Instance.UserDictionary);
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
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            NetworkManager.LoadScene(SceneType.Game, LoadSceneMode.Single, LocalPhysicsMode.Physics3D);
        }
    }
}

