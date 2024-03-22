using UnityEngine;
using Util;

namespace Manager
{
    public class Managers : Singleton<Managers>
    {
        public static Managers Instance = null;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

