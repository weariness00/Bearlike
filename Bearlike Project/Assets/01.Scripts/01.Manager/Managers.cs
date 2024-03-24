using UnityEngine;
using Util;

namespace Manager
{
    public class Managers : Singleton<Managers>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

