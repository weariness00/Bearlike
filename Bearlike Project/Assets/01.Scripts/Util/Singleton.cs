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

        protected virtual void Awake()
        {
            if (_instance != null &&
                _instance.gameObject != gameObject)
            {
                DestroyImmediate(gameObject);
            }
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
    }
}