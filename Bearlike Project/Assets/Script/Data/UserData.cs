using Fusion;
using UnityEngine;

namespace Script.Data
{
    public struct UserDataStruct : INetworkStruct
    {
        [Networked, Capacity(24)] public string Name { get; set; }
        public PlayerRef PlayerRef;
    }

    public class UserData : NetworkBehaviour
    {
        [Networked] [Capacity(3)] [HideInInspector]
        public NetworkDictionary<PlayerRef, UserDataStruct> UserDictionary { get; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

