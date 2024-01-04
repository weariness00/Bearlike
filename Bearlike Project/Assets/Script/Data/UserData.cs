using System;
using System.Collections.Generic;
using Fusion;
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
            UserDictionary.Add(playerRef, userData);
        }

        public void ChangePlayerRef(PlayerRef playerRef,NetworkPrefabRef prefabRef)
        {
            if (UserDictionary.TryGet(playerRef, out UserDataStruct data))
            {
                data.PrefabRef = prefabRef;
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

