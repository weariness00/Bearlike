using System;
using System.Collections;
using System.Threading.Tasks;
using Fusion;
using Script.Manager;
using UnityEngine;

namespace Script.Data
{
    public struct UserDataStruct : INetworkStruct
    {
        public NetworkString<_32> Name { get; set; }
        public PlayerRef PlayerRef;
        public NetworkPrefabRef PrefabRef;
        public NetworkId NetworkId;
        public int ClientNumber;
        [Networked, Capacity(1)]public NetworkLinkedList<Vector3> TeleportPosition { get;}
    }   

    public class UserData : NetworkBehaviour
    {
        public static UserData Instance;
        
        [Networked, Capacity(3)]
        public NetworkDictionary<PlayerRef, UserDataStruct> UserDictionary { get; }

        #region Static Function

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
        
        private void Awake()
        {
            Instance = this;
        }

        public override void Spawned()
        {
            Runner.MakeDontDestroyOnLoad(gameObject);
        }

        public void InsertUserData(PlayerRef playerRef, UserDataStruct userData)
        {
            DebugManager.Log($"Add Player Data : {userData.Name}");
            
            UserDictionary.Add(playerRef, userData);
        }

        public void ChangePlayerRef(PlayerRef playerRef,NetworkPrefabRef prefabRef)
        {
            if (UserDictionary.TryGet(playerRef, out UserDataStruct data))
            {
                DebugManager.Log($"{playerRef}의 캐릭터를 변경");
                data.PrefabRef = prefabRef;
            }
            else
            {
                DebugManager.Log($"{playerRef}가 존재하지 않아 캐릭터를 변경할 수 없습니다.");
            }

            UserDictionary.Set(playerRef, data);
        }

        public async Task<bool> SpawnPlayers()
        {
            if (Runner.IsServer)
            {
                int clientIndex = 0;
                foreach (var (key, value) in UserDictionary)
                {
                    var userDataStruct = value;
                    var spawnObject = await Runner.SpawnAsync(value.PrefabRef, Vector3.zero, Quaternion.identity, value.PlayerRef);
                    
                    userDataStruct.NetworkId = spawnObject.Id;
                    userDataStruct.ClientNumber = clientIndex++;
                    UserDictionary.Remove(key);
                    UserDictionary.Add(key, userDataStruct);
                }
            }

            return true;
        }
    }
}

