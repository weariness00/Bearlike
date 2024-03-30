using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class TsetServer : MonoBehaviour, INetworkRunnerCallbacks
    {
        public struct TestInputData : INetworkInput
        {
            public NetworkBool Click;
            public NetworkBool MoveForward;
            public NetworkBool MoveBack;
            public NetworkBool MoveLeft;
            public NetworkBool MoveRight;

            public NetworkBool Cursor;
            
            public Vector2 MouseAxis;
        }

        private NetworkRunner _runner;
        public SceneReference s;
        public int sceneIndex;
        public NetworkPrefabRef playerRef;

        private void Start()
        {
            Matching(GameMode.AutoHostOrClient, "aa");
        }

        void Matching(GameMode mode, string sessionName)
        {
            // Create the Fusion runner and let it know that we will be providing user inpuz

            gameObject.GetOrAddComponent<RunnerSimulatePhysics3D>();
            _runner = gameObject.GetOrAddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(sceneIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);
            }

            // Start or join (depends on gamemode) a session with a specific name   
            _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = scene,
                MatchmakingMode = MatchmakingMode.FillRoom,
                SceneManager = gameObject.GetOrAddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 3,
                IsVisible = true,
                IsOpen = true,
            });
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var playerInputData = new TestInputData();
            NetworkBool trueValue = true;

            if (Input.GetMouseButtonDown((int)MouseButton.Left))
                playerInputData.Click = trueValue;
            if (Input.GetKeyDown(KeyCode.K))
            {
                playerInputData.Cursor = trueValue;
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                playerInputData.MoveRight = trueValue;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                playerInputData.MoveLeft = trueValue;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                playerInputData.MoveBack = trueValue;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                playerInputData.MoveForward = trueValue;
            }
            
            playerInputData.MouseAxis.x = Input.GetAxis("Mouse X");
            playerInputData.MouseAxis.y = Input.GetAxis("Mouse Y");
            input.Set(playerInputData);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                var obj = runner.Spawn(playerRef, Vector3.zero, quaternion.identity, player);
                runner.SetPlayerObject(player, obj);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
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

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
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

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
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

    }
}