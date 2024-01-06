using System;
using UnityEngine;

namespace Script.Manager
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance;

        public bool isDebug;
        public bool log;
        public bool logWaring;
        public bool logError;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public static void Log(object massage)
        {
            if (!DebugManager.Instance.isDebug && !DebugManager.Instance.log) return;
            Debug.Log(massage);
        }
        public static void LogWaring(object massage)
        {
            if (!DebugManager.Instance.isDebug && !DebugManager.Instance.logWaring) return;
            Debug.LogWarning(massage);
        }
        public static void LogError(object massage)
        {
            if (!DebugManager.Instance.isDebug && !DebugManager.Instance.logError) return;
            Debug.LogError(massage);
        }
        
    }
}

