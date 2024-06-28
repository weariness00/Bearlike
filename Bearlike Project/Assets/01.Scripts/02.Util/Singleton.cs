using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Util
{
    public class Singleton<T> : MonoBehaviour, ISingleton where T : Component, new()
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

        public static bool HasInstance() => _instance;

        public static void Destroy()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
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

                var singletonObject = new GameObject(typeof(T).Name);
                _instance = singletonObject.GetOrAddComponent<T>();
            }
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}