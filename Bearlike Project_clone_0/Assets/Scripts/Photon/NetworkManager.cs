using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Script.Data;
using Script.Manager;
using Script.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.Photon
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkManager Instance;
        
        public NetworkPrefabRef userDataPrefabRef;
        private UserData _userData;
        private NetworkRunner _runner;
    
        private void Awake()
        {
            if(Instance == null) Instance = this;
        }

        private Action<NetworkObject> _IsSetPlayerObjectEvent;
        [HideInInspector]
        public Action<NetworkObject> IsSetPlayerObjectEvent
        {
            get => _IsSetPlayerObjectEvent;
            set
            {
                DebugManager.ToDo($"SetPlayerObject가 호스트에서는 되는데 클라이언트에서는 안되는 이유 찾기");
                if (_runner.TryGetPlayerObject(_runner.LocalPlayer, out var playerObject))
                {
                    StopCoroutine("IsSetPlayerObjectEventCoroutine");
                    value.Invoke(playerObject);
                    return;
                }

                _IsSetPlayerObjectEvent = value;
            }
        }
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

            ObjectUtil.GetORAddComponet<RunnerSimulatePhysics3D>(gameObject);
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

            gameObject.transform.parent = Managers.Instance.transform;

            StartCoroutine(IsSetPlayerObjectEventCoroutine());
        }

        async Task RandomMatching() => await Matching(GameMode.AutoHostOrClient);
        async Task MakeRoom() => await Matching(GameMode.Host);
        async Task JoinRoom() => await Matching(GameMode.Client);

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            var matchManager = FindObjectOfType<MatchManager>();
            var data = new UserDataStruct();

            if (_userData == null)
            {
                _userData = FindObjectOfType<UserData>();
                if(_userData == null)
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

            if (KeyManager.InputActionDown(KeyToAction.Esc))
                OnPlayerLeft(runner, runner.LocalPlayer);
            
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
            DebugManager.Log($"강제 종료 : {runner.LocalPlayer}");
            
            _userData.UserDictionary.Remove(runner.LocalPlayer);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            DebugManager.Log("서버 연결 성공");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            DebugManager.LogWaring($"서버 연결이 끊김\n" +
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