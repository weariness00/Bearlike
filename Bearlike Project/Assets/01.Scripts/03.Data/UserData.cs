using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Manager;
using Photon;
using Player;
using UnityEngine;

namespace Data
{
    public struct UserDataStruct : INetworkStruct
    {
        public NetworkString<_32> Name { get; set; }
        public PlayerRef PlayerRef;
        public NetworkPrefabRef PrefabRef;
        public PlayerCharacterType PlayerCharacterType;
        public NetworkId NetworkId;
        public int ClientNumber;
        [Networked, Capacity(1)] public NetworkLinkedList<Vector3> TeleportPosition { get; }
    }

    public class UserData : NetworkSingleton<UserData>
    {
        [Networked, Capacity(3)] public NetworkDictionary<PlayerRef, UserDataStruct> UserDictionary { get; }

        public Action AfterSpawnedAction;
        public Action<PlayerRef> UserJoinAction;
        public Action<PlayerRef> UserLeftAction;

        public static int ClientNumber => HasClientData(Instance.Runner.LocalPlayer) ? Instance.UserDictionary.Get(Instance.Runner.LocalPlayer).ClientNumber : -1;
        
        #region Static Function

        public static bool HasClientData(PlayerRef playerRef)
        {
            if (Instance.UserDictionary.Count == 0)
                return false;
            var data = Instance.UserDictionary.Get(playerRef);
            return data.PlayerRef != PlayerRef.None;
        }
        public static int GetClientNumber(PlayerRef playerRef) => Instance.UserDictionary.Get(playerRef).ClientNumber;

        public static UserDataStruct[] GetAllUserData()
        {
            UserDataStruct[] datas = new UserDataStruct[Instance.UserDictionary.Count];
            int i = 0;
            foreach (var (key, value) in Instance.UserDictionary)
                datas[i++] = value;
            return datas;
        }
        
        public static void SetTeleportPosition(PlayerRef key, Vector3? value)
        {
            var data = Instance.UserDictionary[key];

            data.TeleportPosition.Clear();
            if (value != null)
            {
                data.TeleportPosition.Add(value.Value);
            }

            Instance.UserDictionary.Set(key, data);
        }

        #endregion

        public override void Spawned()
        {
            Runner.MakeDontDestroyOnLoad(gameObject);
            AfterSpawnedAction?.Invoke();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void UserLeftRPC(PlayerRef playerRef) => UserLeft(playerRef);

        public void UserLeft(PlayerRef playerRef)
        {
            if (UserDictionary.TryGet(playerRef, out var data))
            {
                UserDictionary.Remove(playerRef);
                UserLeftAction?.Invoke(playerRef);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void InsertUserDataRPC(PlayerRef playerRef, UserDataStruct userData) => InsertUserData(playerRef, userData);

        public void InsertUserData(PlayerRef playerRef, UserDataStruct userData)
        {
            try
            {
                var matchManager = FindObjectOfType<NetworkMatchManager>();

                DebugManager.ToDo("임시 : 나중에는 유저가 선택하면 바꿀 수 있거나 아니면 이전 정보를 가져와 그 캐릭터로 잡아줌");
                userData.PrefabRef = matchManager.PlayerPrefabRefs[0];
                userData.ClientNumber = Runner.ActivePlayers.ToArray().Length - 1;

                UserDictionary.Add(playerRef, userData);
                UserJoinAction?.Invoke(playerRef);
                DebugManager.Log($"Add Player Data : {userData.Name}");
            }
            catch (Exception e)
            {
                AfterSpawnedAction += () => InsertUserDataRPC(playerRef, userData);
            }
        }

        public void ChangePlayerRef(PlayerRef playerRef, NetworkPrefabRef prefabRef, PlayerCharacterType playerType)
        {
            if (UserDictionary.TryGet(playerRef, out UserDataStruct data))
            {
                DebugManager.Log($"{playerRef}의 캐릭터를 변경");
                data.PrefabRef = prefabRef;
                data.PlayerCharacterType = playerType;
                
                UserDictionary.Set(playerRef, data);
            }
            else
            {
                DebugManager.Log($"{playerRef}가 존재하지 않아 캐릭터를 변경할 수 없습니다.");
            }
        }

        public async Task<bool> SpawnPlayers()
        {
            if (Runner.IsServer)
            {
                int clientIndex = 0;
                foreach (var (playerRef, data) in UserDictionary)
                {
                    var userDataStruct = data;
                    var spawnObject = await Runner.SpawnAsync(data.PrefabRef, Vector3.zero, Quaternion.identity, data.PlayerRef);

                    userDataStruct.NetworkId = spawnObject.Id;
                    userDataStruct.ClientNumber = clientIndex++;
                    UserDictionary.Remove(playerRef);
                    UserDictionary.Add(playerRef, userDataStruct);

                    Runner.SetPlayerObject(playerRef, spawnObject);
                }
            }

            return true;
        }

        #region Rpc Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void ChangePlayerRefRPC(PlayerRef playerRef, NetworkPrefabRef prefabRef, PlayerCharacterType playerType) => ChangePlayerRef(playerRef, prefabRef, playerType);

        #endregion
    }
}