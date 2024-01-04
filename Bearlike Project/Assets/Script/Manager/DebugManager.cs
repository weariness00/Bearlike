using System;
using UnityEngine;

namespace Script.Manager
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance;

        public bool isBuild;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public static void Log(object massage)
        {
            if (DebugManager.Instance.isBuild) return;
            Debug.Log(massage);
        }
        public static void LogWaring(object massage)
        {
            if (DebugManager.Instance.isBuild) return;
            Debug.LogWarning(massage);
        }
        public static void LogError(object massage)
        {
            if (DebugManager.Instance.isBuild) return;
            Debug.LogError(massage);
        }
        
    }
}

