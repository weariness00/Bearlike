﻿using UnityEngine;
using Util;

namespace Manager
{
    public class DebugManager : Singleton<DebugManager>
    {
        public bool isDebug = true;
        public bool log = true;
        public bool logWaring = true;
        public bool logError = true;
        public bool drawRay = true;

        [Header("TO DO")] 
        public bool isToDo = true;

        #region Log

        public static void Log(object massage)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.log) return;
            Debug.Log(massage);
        }

        public static void LogWarning(object massage)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.logWaring) return;
            Debug.LogWarning(massage);
        }

        public static void LogError(object massage)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.logError) return;
            Debug.LogError(massage);
        }

        #endregion

        #region TODO Log

        public static void ToDo(object massage)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.isToDo) return;
            Debug.Log("TO DO List\n" + massage);
        }

        #endregion

        public static void DrawRay(Vector3 position, Vector3 direction, Color color, float time)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.drawRay) return;
            Debug.DrawRay(position, direction, color, time);
        }

        public static void DrawSphereRay(Vector3 position, Vector3 direction, float radius, Color color,  float time = 1f)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.drawRay) return;
            var plusPosition = position * radius;
            var minusPosition = -position * radius;
            Debug.DrawLine(new Vector3(plusPosition.x, position.y, position.z), new Vector3(minusPosition.x, position.y, position.z));
            Debug.DrawLine(new Vector3(position.x, plusPosition.y, position.z), new Vector3(position.x, minusPosition.y, position.z));
            Debug.DrawLine(new Vector3(position.x, position.y, plusPosition.z), new Vector3(position.x, position.y, minusPosition.z));
        }
    }
}