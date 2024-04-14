using System;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Photon
{
    public class NetworkSingleton<T> : NetworkBehaviourEx, ISingleton where T : Component, new()
    {
        public static T Instance
        {
            get
            {
                Init();
                return _instance;
            }
        }
        private static T _instance = null;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }

            Init();
        }

        private static void Init()
        {
            if (_instance == null)
            {
                var componet = FindObjectOfType<T>();
                if (componet != null)
                {
                    _instance = componet;
                    return;
                }
            }
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}