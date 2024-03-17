using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Manager;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon
{
    public class NetworkManager : Util.Singleton<NetworkManager>, INetworkRunnerCallbacks
    {
        public static NetworkRunner Runner => Instance._runner;
        public static int PlayerCount => Runner.ActivePlayers.ToArray().Length;

        public bool isTest = true; // 현재 테스트 상황인지
        public SceneReference lobbyScene;
        private string[] _sessionNames;
        private NetworkRunner _runner;

        public bool isCursor;
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

        #region Session Info

        public struct RoomInfo : IEnumerable<RoomInfo>
        {
            [JsonProperty("Session Name")]public string Name;
            [JsonProperty("Player Count")]public int PlayerCount;
            [JsonProperty("Is Game Start")]public bool IsGameStart;

            public RoomInfo(string name)
            {
                Name = name;
                PlayerCount = 1;
                IsGameStart = false;
            }

            public IEnumerator<RoomInfo> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        #endregion

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

        #region Scene Static Funtion

        public static Action SceneLoadDoneAction 
        {
            get => Instance._sceneLoadDoneAction;
            set
            {
                if (Instance._isLoadDone)
                {
                    value?.Invoke();
                }
                else
                {
                    Instance._sceneLoadDoneAction = value;
                }
            }
            
        }
        private Action _sceneLoadDoneAction;
        private bool _isLoadDone = false;
        IEnumerator LoadSceneDoneCoroutine()
        {
            while (_isLoadDone == false)
            {
                yield return null;
            }
            SceneLoadDoneAction?.Invoke();
            SceneLoadDoneAction = null;
        }
        
        public static async Task LoadScene(SceneRef sceneRef, LoadSceneParameters parameters, bool setActiveOnLoad = false)
        {
            if (Instance._runner.IsSceneAuthority)
            {
                if (parameters.loadSceneMode == LoadSceneMode.Single)
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

        async Task Matching(GameMode mode, string sessionName)
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
            
            // Start or join (depends on gamemode) a session with a specific name   
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "ssssss",
                Scene = scene,
                MatchmakingMode = MatchmakingMode.FillRoom,
                SceneManager = gameObject.GetOrAddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 3,
                IsVisible = true,
                IsOpen = true,
            });

            gameObject.transform.parent = Managers.Instance.transform;

            StartCoroutine(IsSetPlayerObjectEventCoroutine());
        }

        async Task RandomMatching()
        {
            DebugManager.ToDo("세션에 인원이 꽉 차면 들어가지 못하게 해야됨\n" +
                              "세션에 인원이 없으면 들어 갈 수 있게 해야됨\n" +
                              "누군가가 세션을 나가면 정보 업데이트 해줘야함\n" +
                              "세션에 한명도 없으면 json에 세션 지워줘야함");
            await Matching(GameMode.AutoHostOrClient, "asd");
            
            // WebManager.DownloadJson(WebURL.RoomURL, "", async (json) =>
            // {
            //     RoomInfo[] roomInfos = JsonConvert.DeserializeObject<RoomInfo[]>(json);
            //     var sessionNames = roomInfos.Select(room => room.Name).ToArray();
            //     var sessionName = GetRandomSessionName(sessionNames);
            //     await Matching(GameMode.AutoHostOrClient, sessionName);
            // }, false, false);
        }

        async Task MakeRoom()
        {
            // var fileName = "Lobby Session";
            // string createSessionName = null;
            // while (true)
            // {
            //     createSessionName = null;
            //     ProjectUpdateManager.DownLoadLobbyToStorage(fileName);
            //     JsonConvertExtension.Load(fileName, (data) =>
            //     {
            //         var sessionInfos = JsonConvert.DeserializeObject<SessionInfo[]>(data);
            //         var sessionNames = sessionInfos.Select(info => info.Name).ToArray();
            //         createSessionName = GetRandomSessionName(sessionNames);
            //         var newSessionInfo = new SessionInfo(createSessionName);
            //         sessionInfos.AddRange(newSessionInfo);
            //
            //         JsonConvertExtension.Save(JsonConvert.SerializeObject(sessionInfos), fileName);
            //     });
            //     if (ProjectUpdateManager.UploadJsonToStorage(fileName))
            //     {
            //         break;
            //     }
            // }
            //
            // if (createSessionName == null)
            // {
            //     DebugManager.LogError("서버와의 연결 상태가 좋지 않습니다. 다시 시도해주세요");
            //     return;
            // }
            //
            // await Matching(GameMode.Host, createSessionName);
            await Matching(GameMode.Host, "a");
        }

        async Task JoinRoom()
        {
            DebugManager.ToDo("세션에 인원이 꽉 차면 들어가지 못하게 해야됨\n" +
                              "세션에 인원이 없으면 들어 갈 수 있게 해야됨\n" +
                              "누군가가 세션을 나가면 정보 업데이트 해줘야함\n" +
                              "세션에 한명도 없으면 json에 세션 지워줘야함");
            // var fileName = "Lobby Session";
            // string createSessionName = null;
            // ProjectUpdateManager.DownLoadLobbyToStorage(fileName);
            // JsonConvertExtension.Load(fileName, (data) =>
            // {
            //     var sessionInfos = JsonConvert.DeserializeObject<SessionInfo[]>(data);
            //     var sessionNames = sessionInfos.Select(info => info.Name).ToArray();
            //     createSessionName = GetRandomSessionName(sessionNames);
            //     var newSessionInfo = new SessionInfo(createSessionName);
            //     sessionInfos.AddRange(newSessionInfo);
            //
            //     JsonConvertExtension.Save(JsonConvert.SerializeObject(sessionInfos), fileName);
            // });
            // if (ProjectUpdateManager.UploadJsonToStorage(fileName))
            // {
            //     break;
            // }
            //
            // if (createSessionName == null)
            // {
            //     DebugManager.LogError("서버와의 연결 상태가 좋지 않습니다. 다시 시도해주세요");
            //     return;
            // }
            //
            // await Matching(GameMode.Client, );
            await Matching(GameMode.Client, "a");
        }

        private bool IsValidSessionName(string[] sessionNames, string searchSessionName)
        {
            foreach (var sessionName in sessionNames)
            {
                if (searchSessionName.Equals(sessionName))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Network Callback Function

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Physics.SyncTransforms();
            
            var data = new UserDataStruct
            {
                PlayerRef = player,
                Name = player.ToString()
            };

            UserData.Instance.InsertUserDataRPC(player, data);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            DebugManager.Log($"종료 : {player}");

            UserData.Instance.UserDictionary.Remove(player);
            UserData.Instance.UserLeftRPC(player);
            
            if (runner.ActivePlayers.ToList().Count == 0)
            {
                runner.Shutdown();
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var playerInputData = new PlayerInputData();
            NetworkBool trueValue = true;
            
            // 마우스 휠 클릭시 UI와 상호작용 할 수 있도록 플레이어 정지
            if (KeyManager.InputActionDown(KeyToAction.Esc) || KeyManager.InputActionDown(KeyToAction.LockCursor))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
                isCursor = !isCursor;
            }
            if (isCursor)
            {
                playerInputData.Cursor = trueValue;
                input.Set(playerInputData);
                return;
            }

            if (KeyManager.InputAction(KeyToAction.MoveFront))
                playerInputData.MoveFront = trueValue;
            if (KeyManager.InputAction(KeyToAction.MoveBack))
                playerInputData.MoveBack = trueValue;
            if (KeyManager.InputAction(KeyToAction.MoveLeft))
                playerInputData.MoveLeft = trueValue;
            if (KeyManager.InputAction(KeyToAction.MoveRight))
                playerInputData.MoveRight = trueValue;
            
            if (KeyManager.InputAction(KeyToAction.Jump))
            {
                playerInputData.Jump = trueValue;
            }
            
            if (KeyManager.InputActionDown(KeyToAction.Reload))
                playerInputData.ReLoad = trueValue;
            
            if (KeyManager.InputAction(KeyToAction.Attack))
                playerInputData.Attack = trueValue;

            if (KeyManager.InputAction(KeyToAction.FirstSkill))
                playerInputData.FirstSkill = trueValue;
            if (KeyManager.InputAction(KeyToAction.Ultimate))
                playerInputData.Ultimate = trueValue;

            if (KeyManager.InputAction(KeyToAction.Interact) || KeyManager.InputActionDown(KeyToAction.Interact))
                playerInputData.Interact = trueValue;
            
            playerInputData.MouseAxis.x = Input.GetAxis("Mouse X");
            playerInputData.MouseAxis.y = Input.GetAxis("Mouse Y");

            input.Set(playerInputData);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            var player = runner.LocalPlayer;
            DebugManager.Log($"서버 종료 : {runner.LocalPlayer}");

            SceneManager.LoadScene(lobbyScene.ScenePath);

            UserData.Instance.UserDictionary.Remove(runner.LocalPlayer);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            DebugManager.Log("서버 연결 성공\n" +
                             $"세션 이름 : {runner.SessionInfo.Name}");
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

        public void OnSessionListUpdated(NetworkRunner runner, List<Fusion.SessionInfo> sessionList)
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
            DebugManager.Log($"씬 Loading 끝");
            _isLoadDone = true;
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            DebugManager.Log($"씬 Loading 중");
            _isLoadDone = false;
            StartCoroutine(LoadSceneDoneCoroutine());
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