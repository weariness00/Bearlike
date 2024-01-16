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
    }   

    public class UserData : NetworkBehaviour
    {
        public static UserData Instance;
        
        [Networked, Capacity(3)]
        public NetworkDictionary<PlayerRef, UserDataStruct> UserDictionary { get; }

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
                foreach (var (key, value) in UserDictionary)
                {
                    var userDataStruct = value;
                    var spawnObject = await Runner.SpawnAsync(value.PrefabRef, Vector3.zero, Quaternion.identity, value.PlayerRef);
                    
                    userDataStruct.NetworkId = spawnObject.Id;
                    UserDictionary.Remove(key);
                    UserDictionary.Add(key, userDataStruct);
                }
            }

            return true;
        }
    }
}

