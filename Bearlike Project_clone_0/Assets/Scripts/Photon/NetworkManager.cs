﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Script.Data;
using Script.Manager;
using Script.Photon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon
{
    public class NetworkManager : global::Util.Singleton<NetworkManager>, INetworkRunnerCallbacks
    {
        public static NetworkRunner Runner => Instance._runner;
        
        public NetworkPrefabRef userDataPrefabRef;
        private UserData _userData;
        private NetworkRunner _runner;

        private Action<NetworkObject> _isSetPlayerObjectEvent;

        public Action<NetworkObject> IsSetPlayerObjectEvent
        {
            get => _isSetPlayerObjectEvent;
            set
            {
                DebugManager.ToDo($"SetPlayerObject가 호스트에서는 되는데 클라이언트에서는 안되는 이유 찾기");
                if (_runner.TryGetPlayerObject(_runner.LocalPlayer, out var playerObject))
                {
                    StopCoroutine("IsSetPlayerObjectEventCoroutine");
                    value.Invoke(playerObject);
                    return;
                }

                _isSetPlayerObjectEvent = value;
            }
        }

        #region Scene Static Funtion

        public static async Task LoadScene(SceneRef sceneRef, LoadSceneParameters parameters, bool setActiveOnLoad = false)
        {
            if (Instance._runner.IsSceneAuthority)
            {
                if(parameters.loadSceneMode == LoadSceneMode.Single)
                {
                    Instance._runner.LoadScene(sceneRef, parameters, setActiveOnLoad);
                }
                else
                {
                    await Instance._runner.LoadScene(sceneRef, parameters, setActiveOnLoad);
                }
                DebugManager.Log($"씬 불러오기 성공 : {sceneRef}");
            }
        }
        
        public static async Task LoadScene(SceneType type, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool setActiveOnLoad = false)
        {
            DebugManager.ToDo("나중에 씬 호출을 에셋 번들로 바꾸기");
            if (Instance._runner.IsSceneAuthority)
            {
                LoadSceneParameters sceneParameters = new LoadSceneParameters()
                {
                    loadSceneMode = sceneMode,
                    localPhysicsMode = physicsMode,
                };
                await NetworkManager.LoadScene(type, sceneParameters, setActiveOnLoad);
            }
        }

        public static Task LoadScene(SceneType type, LoadSceneParameters parameters, bool setActiveOnLoad = false) => LoadScene(SceneRef.FromIndex((int)type), parameters, setActiveOnLoad);
        public static Task LoadScene(int type, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool setActiveOnLoad = false) => LoadScene((SceneType)type, sceneMode, physicsMode, setActiveOnLoad);
        
        public static Task LoadScene(string path, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool setActiveOnLoad = false) => LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(path)), new LoadSceneParameters(sceneMode, physicsMode), setActiveOnLoad);
        public static Task LoadScene(string path, LoadSceneParameters parameters, bool setActiveOnLoad = false) => LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(path)), parameters, setActiveOnLoad); 

        public static async void UnloadScene(SceneRef sceneRef)
        {
            if (Instance._runner.IsSceneAuthority)
            {
                await Instance._runner.UnloadScene(sceneRef);
            }
        }
        public static void UnloadScene(string path) => UnloadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(path))); 
        public static void UnloadScene(SceneType type) => UnloadScene(SceneRef.FromIndex((int)type)); 

        #endregion

        #region Network Connect Function

        IEnumerator IsSetPlayerObjectEventCoroutine()
        {
            while (true)
            {
                if (_runner.TryGetPlayerObject(_runner.LocalPlayer, out var playerObject))
                {
                    IsSetPlayerObjectEvent?.Invoke(playerObject);
                    break;
                }

                yield return null;
            }
        }

        async Task Matching(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user inpuz

            gameObject.GetOrAddComponent<RunnerSimulatePhysics3D>();
            _runner = gameObject.GetOrAddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex((int)SceneType.Matching);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }
            
            DebugManager.ToDo("게임 세션을 만들때 구글 스토리지를 통해 만들어진 섹션 정보 얻어올 수 있게 하기");

            // Start or join (depends on gamemode) a session with a specific name   
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "Game Room",
                Scene = scene,
                SceneManager = gameObject.GetOrAddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 3,
                IsVisible = true,
                IsOpen = true,
            });

            gameObject.transform.parent = Managers.Instance.transform;

            StartCoroutine(IsSetPlayerObjectEventCoroutine());
        }

        async Task RandomMatching() => await Matching(GameMode.AutoHostOrClient);
        async Task MakeRoom() => await Matching(GameMode.Host);
        async Task JoinRoom() => await Matching(GameMode.Client);

        #endregion

        #region Network Callback Function

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            var matchManager = FindObjectOfType<NetworkMatchManager>();
            var data = new UserDataStruct();

            if (_userData == null)
            {
                _userData = FindObjectOfType<UserData>();
                if (_userData == null)
                    _userData = _runner.Spawn(userDataPrefabRef, Vector3.zero, Quaternion.identity).GetComponent<UserData>();
            }

            data.PlayerRef = player;
            data.Name = player.ToString();
            data.PrefabRef = matchManager.PlayerPrefabRefs[0]; // 임시 : 나중에는 유저가 선택하면 바꿀 수 있거나 아니면 이전 정보를 가져와 그 캐릭터로 잡아줌

            _userData.InsertUserData(player, data);
            matchManager.DataUpdate();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            DebugManager.Log($"종료 : {player}");

            _userData.UserDictionary.Remove(player);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var playerInputData = new PlayerInputData();

            if (KeyManager.InputAction(KeyToAction.MoveFront))
                playerInputData.MoveFront = true;
            if (KeyManager.InputAction(KeyToAction.MoveBack))
                playerInputData.MoveBack = true;
            if (KeyManager.InputAction(KeyToAction.MoveLeft))
                playerInputData.MoveLeft = true;
            if (KeyManager.InputAction(KeyToAction.MoveRight))
                playerInputData.MoveRight = true;

            if (KeyManager.InputActionDown(KeyToAction.ReLoad))
                playerInputData.ReLoad = true;

            if (KeyManager.InputAction(KeyToAction.Attack))
                playerInputData.Attack = true;

            // if (KeyManager.InputActionDown(KeyToAction.Esc))
            //     OnPlayerLeft(runner, runner.LocalPlayer);

            playerInputData.MouseAxis.x = Input.GetAxis("Mouse X");
            playerInputData.MouseAxis.y = Input.GetAxis("Mouse Y");

            input.Set(playerInputData);
            
            if (Input.GetMouseButtonDown((int)MouseButton.Middle))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None; 
            }
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            var player = runner.LocalPlayer;
            DebugManager.Log($"강제 종료 : {runner.LocalPlayer}");

            _userData.UserDictionary.Remove(runner.LocalPlayer);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            DebugManager.Log("서버 연결 성공");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            DebugManager.LogWarning($"서버 연결이 끊김\n" +
                                    $"서버 이름 : {runner.SceneManager.MainRunnerScene.name}");
            SceneManager.LoadScene((int)SceneType.Lobby);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            DebugManager.LogError($"서버 연결 실패 : {reason}");
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
            DebugManager.Log($"씬 Loading 끝 : {runner.SceneManager.MainRunnerScene.name}");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            DebugManager.Log($"씬 Loading 중");
            
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

        #endregion
    }
}