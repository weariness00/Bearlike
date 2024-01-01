using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Script.Data;
using Script.Manager;
using Script.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Photon
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 direction;
    }
    
    public class MatchNetwork : MonoBehaviour, INetworkRunnerCallbacks
    {
        public int count = 0;

        public NetworkPrefabRef userDataPrefabRef;
        public UserData userData;
        private NetworkRunner _runner;

        private void Start()
        {
            userData = FindObjectOfType<UserData>();
        }

        async void Matching(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            
            _runner = ObjectUtil.GetORAddComponet<NetworkRunner>(gameObject);
            _runner.ProvideInput = true;

            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex((int)SceneType.Matching);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)  
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            // Start or join (depends on gamemode) a session with a specific name   
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "Matching Room",
                Scene = scene,
                SceneManager = ObjectUtil.GetORAddComponet<NetworkSceneManagerDefault>(gameObject)
            });
        }

        async void MakeRoom() => Matching(GameMode.Host);
        async void JoinRoom() => Matching(GameMode.Client);

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            count++;
            var matchManager = FindObjectOfType<MatchManager>();
            if (runner.IsServer)
            {
                userData = _runner.Spawn(userDataPrefabRef, Vector3.zero, Quaternion.identity).GetComponent<UserData>();
                var data = new UserDataStruct();
                data.PlayerRef = player;
                data.Name = player.ToString(); 
                // userData.InsertUserData(player, data);
            }
            matchManager.DataUpdate();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            count--;
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            
            if (Input.GetKey(KeyCode.W))
                data.direction += Vector3.forward;
            
            if (Input.GetKey(KeyCode.S))
                data.direction += Vector3.back;
            
            if (Input.GetKey(KeyCode.A))
                data.direction += Vector3.left;
            
            if (Input.GetKey(KeyCode.D))
                data.direction += Vector3.right;
            
            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }
    }
}