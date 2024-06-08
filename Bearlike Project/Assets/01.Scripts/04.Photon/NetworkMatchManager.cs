using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Fusion;
using GamePlay;
using Loading;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace Photon
{
    public class NetworkMatchManager : NetworkBehaviour
    {
        public MatchRoomUserUI roomUserUI;
        public SceneReference playerJoinLoadingScene;

        public List<NetworkPrefabRef> PlayerPrefabRefs;

        public PlayerCharacterType currenPlayerCharacterType = PlayerCharacterType.FirstBear;
        public GameObject[] playerModels;

        public void Awake()
        {
            LoadingManager.Initialize();
            LoadingManager.StartAction += ()=> SceneManager.LoadScene(playerJoinLoadingScene, LoadSceneMode.Additive);
            LoadingManager.EndAction += () => StartCoroutine(LoadingUnload());
        }

        public override void Spawned()
        {
            StartCoroutine(InitCoroutine());
        }

        public void NextPlayerPrefab()
        {
            var userData = FindObjectOfType<UserData>();

            currenPlayerCharacterType = currenPlayerCharacterType.Next();

            userData.ChangePlayerRefRPC(Runner.LocalPlayer, PlayerPrefabRefs[(int)currenPlayerCharacterType], currenPlayerCharacterType);
            ChangeCharacterRPC(UserData.ClientNumber, currenPlayerCharacterType);
        }
        
        async void GameStart()
        {
            gameObject.SetActive(false);
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            SetDifficultRPC(roomUserUI.GetDifficult());
            
            await NetworkManager.LoadScene(SceneType.Game, LoadSceneMode.Single, LocalPhysicsMode.Physics3D);
        }

        private IEnumerator InitCoroutine()
        {
            while (!UserData.Instance)
                yield return null;
            while (UserData.HasClientData(Runner.LocalPlayer) == false)
                yield return null;

            var datas = UserData.GetAllUserData();
            foreach (var data in datas)
            {
                ChangeCharacterRPC(data.ClientNumber, data.PlayerCharacterType);
            }
        }
        
        private IEnumerator LoadingUnload()
        {
            while (true)
            {
                Scene s = SceneManager.GetSceneByPath(playerJoinLoadingScene);
                if (s.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(playerJoinLoadingScene);
                    break;
                }

                yield return null;
            }
        }

        #region Rpc Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ChangeCharacterRPC(int clientNumber, PlayerCharacterType type)
        {
            var targetPlayerModels = playerModels[clientNumber];
            for (int i = 0; i < targetPlayerModels.transform.childCount; i++)
                targetPlayerModels.transform.GetChild(i).gameObject.SetActive(false);
            targetPlayerModels.transform.GetChild((int)type).gameObject.SetActive(true);
        }
        
        [Rpc(RpcSources.All,RpcTargets.All)]
        public void SetDifficultRPC(string diffName) => Difficult.InitDifficult(diffName);

        #endregion
    }
}

