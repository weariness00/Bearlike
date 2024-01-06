using System;
using System.Collections.Generic;
using Fusion;
using Script.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Data
{
    public struct UserDataStruct : INetworkStruct
    {
        public NetworkString<_32> Name { get; set; }
        public PlayerRef PlayerRef;
        public NetworkPrefabRef PrefabRef;
    }   

    public class UserData : NetworkBehaviour
    {
        [Networked, Capacity(3)]
        public NetworkDictionary<PlayerRef, UserDataStruct> UserDictionary { get; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
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

        public void SpawnPlayers()
        {
            foreach (var (key, value) in UserDictionary)
            {
                Runner.SpawnAsync(value.PrefabRef, Vector3.zero, Quaternion.identity, value.PlayerRef);
            }
        }
    }
}

