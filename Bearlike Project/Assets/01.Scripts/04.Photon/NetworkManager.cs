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
using Loading;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace Photon
{
    [Serializable]
    public enum NetworkState
    {
        None,
        JoinReady,
        JoinSuccess,
    }
    
    public class NetworkManager : Util.Singleton<NetworkManager>, INetworkRunnerCallbacks
    {
        public static NetworkRunner Runner => Instance._runner;
        public static int PlayerCount => Runner.ActivePlayers.ToArray().Length;

        public bool isTest = true; // 현재 테스트 상황인지

        public SceneReference matchingScene;
        public SceneReference lobbyScene;
        
        private NetworkRunner _runner;
        private SessionInfo[] _sessionInfoAll = Array.Empty<SessionInfo>();

        public bool isCursor;

        public Action<SessionInfo[]> SessionListUpdateAction;

        private TickTimer _keyDownTimer;

        #region Unity Event Function

        protected override void Awake()
        {
            base.Awake();
            // Create the Fusion runner and let it know that we will be providing user inpuz
            gameObject.GetOrAddComponent<RunnerSimulatePhysics3D>();
            _runner = gameObject.GetOrAddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            _runner.AddCallbacks(this);
        }

        #endregion

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
            yield return null;
            SceneLoadDoneAction?.Invoke();
            SceneLoadDoneAction = null;
        }

        IEnumerator JoinPlayerCoroutine(NetworkRunner runner, PlayerRef player)
        {
            while (UserData.Instance == null)
                yield return null;
            
            var data = new UserDataStruct
            {
                PlayerRef = player,
                Name = player.ToString()
            };
            UserData.Instance.InsertUserDataRPC(player, data);
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

        #endregion

        #region Network Connect Function

        public async void LobbyConnect()
        {
            if (_runner.IsRunning)
            {
                DebugManager.LogWarning("기존 연결 종료 중...");
                await _runner.Shutdown();
                DebugManager.LogWarning("기존 연결 종료 완료");
            }
            
            await _runner.JoinSessionLobby(SessionLobby.Shared);
        }
        
        async Task Matching(GameMode mode, string sessionName)
        {
            DebugManager.ToDo("세션에 인원이 꽉 차면 들어가지 못하게 해야됨\n" +
                              "세션에 인원이 없으면 들어 갈 수 있게 해야됨\n" +
                              "누군가가 세션을 나가면 정보 업데이트 해줘야함\n" +
                              "세션에 한명도 없으면 json에 세션 지워줘야함");

            LoadingManager.Initialize();
            
            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(matchingScene));
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }
            
            // Start or join (depends on gamemode) a session with a specific name   
            var result = await _runner.StartGame(new StartGameArgs()
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

            if (result.Ok)
                DebugManager.Log("게임 정상 시작");
            else
            {
                DebugManager.LogError($"게임 시작 실패\n{result.ShutdownReason}");
                if (result.ShutdownReason == ShutdownReason.Error)
                    DebugManager.LogError($"게임 시작 에러: {result.ErrorMessage}");
                LobbyConnect();
                return;
            }

            gameObject.transform.parent = Managers.Instance.transform;
            
            _keyDownTimer = TickTimer.CreateFromTicks(_runner, 2);
        }

        async Task RandomMatching()
        {
            var sessionNames = _sessionInfoAll.Where(info => info.IsOpen && info.PlayerCount < info.MaxPlayers).Select(info => info.Name).ToArray();
            var sessionName = sessionNames.Length == 0 ? MakeSessionName(_sessionInfoAll.Select(info => info.Name).ToArray()) : sessionNames[UnityEngine.Random.Range(0, sessionNames.Length)];
            
            await Matching(GameMode.AutoHostOrClient, sessionName);
        }

        public async Task MakeRoom()
        {
            var sessionName = MakeSessionName(_sessionInfoAll.Select(info => info.Name).ToArray());

            await Matching(GameMode.Host, sessionName);
        }

        public async Task JoinRoom(string sessionName)
        {
            await Matching(GameMode.Client, sessionName);
        }


        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public static string MakeSessionName(string[] sessionNames)
        {
            string randomName = string.Empty;
            bool isSuccess = false;

            while (!isSuccess)
            {
                randomName = RandomString(6);
                isSuccess = true;
            
                foreach (var sessionName in sessionNames)
                {
                    if (randomName.Equals(sessionName, StringComparison.OrdinalIgnoreCase))
                    {
                        isSuccess = false;
                        break;
                    }
                }
            }

            return randomName;
        }

        private static string RandomString(int length)
        {
            Random random = new Random();
            string result = string.Empty;
        
            for (int i = 0; i < length; i++)
            {
                result += Characters[random.Next(Characters.Length)];
            }

            return result;
        }

        #endregion

        #region Network Callback Function

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if(runner.IsServer)
                LoadingManager.AddWait();

            StartCoroutine(JoinPlayerCoroutine(runner, player));
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

            if (LoadingManager.IsLoading)
            {
                input.Set(playerInputData);
                return;
            }
            
            // 마우스 휠 클릭시 UI와 상호작용 할 수 있도록 플레이어 정지
            if (_keyDownTimer.Expired(runner))
            {
                if (KeyManager.InputActionDown(KeyToAction.LockCursor))
                {
                    switch (Cursor.lockState)
                    {
                        case CursorLockMode.None:
                            Cursor.lockState = CursorLockMode.Locked;
                            isCursor = true;
                            break;
                        case CursorLockMode.Locked:
                            Cursor.lockState = CursorLockMode.None;
                            isCursor = false;
                            break;
                    }
                    isCursor = !isCursor;

                    _keyDownTimer = TickTimer.CreateFromTicks(runner, 2);
                }
                if (KeyManager.InputAction(KeyToAction.Esc))
                {
                    playerInputData.Escape = true;
                    _keyDownTimer = TickTimer.CreateFromTicks(runner, 2);
                }
            }
            if (Cursor.lockState == CursorLockMode.None)
                playerInputData.Cursor = trueValue;

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

            if (KeyManager.InputActionDown(KeyToAction.FirstSkill))
                playerInputData.FirstSkill = trueValue;
            if (KeyManager.InputActionDown(KeyToAction.SecondSkill))
                playerInputData.SecondSkill = trueValue;
            if (KeyManager.InputActionDown(KeyToAction.Ultimate))
                playerInputData.Ultimate = trueValue;

            if (KeyManager.InputActionDown(KeyToAction.ChangeWeapon0))
                playerInputData.ChangeWeapon0 = trueValue;
            else if (KeyManager.InputActionDown(KeyToAction.ChangeWeapon1))
                playerInputData.ChangeWeapon1 = trueValue;
            else if (KeyManager.InputActionDown(KeyToAction.ChangeWeapon2))
                playerInputData.ChangeWeapon2 = trueValue;

            if (KeyManager.InputActionDown(KeyToAction.StageSelect))
                playerInputData.StageSelect = trueValue;
            if (KeyManager.InputActionDown(KeyToAction.ItemInventory))
                playerInputData.ItemInventory = trueValue;
            if (KeyManager.InputActionDown(KeyToAction.SkillInventory))
                playerInputData.SkillInventory = trueValue;
            if (KeyManager.InputActionDown(KeyToAction.SkillSelect))
                playerInputData.SkillSelect = trueValue;
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
            
            if(shutdownReason == ShutdownReason.Ok)
                DebugManager.Log($"서버 종료 : {runner.LocalPlayer}");
            else if(shutdownReason == ShutdownReason.Error)
                DebugManager.LogError($"비정상 서버 종료\n {runner}");

            SceneManager.LoadScene(lobbyScene.ScenePath);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            DebugManager.Log("서버 연결 성공\n" +
                             $"세션 이름 : {runner.SessionInfo.Name}");

            _keyDownTimer = TickTimer.CreateFromTicks(runner, 2);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            DebugManager.LogWarning($"서버 연결이 끊김\n" +
                                    $"서버 이름 : {runner.SceneManager.MainRunnerScene.name}");
            SceneManager.LoadScene(lobbyScene);
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
            _sessionInfoAll = sessionList.ToArray();
            
            foreach (SessionInfo session in sessionList)
            {
                DebugManager.Log($"Session: {session.Name}, Players: {session.PlayerCount}");
            }
            
            SessionListUpdateAction?.Invoke(_sessionInfoAll);
            DebugManager.Log("세션 리스트 업데이트");
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