using UnityEngine;

namespace Manager
{
    public class Managers : MonoBehaviour
    {
        public static Managers Instance = null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

