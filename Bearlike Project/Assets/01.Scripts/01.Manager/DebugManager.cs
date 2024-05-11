using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Manager
{
    public class DebugManager : Singleton<DebugManager>
    {
        public bool isDebug = true;
        
        public bool log = true;
        public bool logWaring = true;
        public bool logError = true;
        
        public bool isToDo = true;

        public bool drawRay = true;
        public bool drawBoxRay = true;


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

        #region Ray 

        public static void DrawRay(Vector3 position, Vector3 direction, Color color, float time)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.drawRay) return;
            Debug.DrawRay(position, direction, color, time);
        }

        public static void DrawBoxRay(
            Vector3 center,
            Vector3 halfExtents,
            Vector3 direction,
            Quaternion orientation,
            float distance,
            Color color,
            float duration = 1f)
        {
            if (!DebugManager.Instance.isDebug || !DebugManager.Instance.drawBoxRay) return;
            Vector3 scaledDirection = direction.normalized * distance;
            Vector3 right = orientation * Vector3.right * halfExtents.x;
            Vector3 up = orientation * Vector3.up * halfExtents.y;
            Vector3 forward = orientation * Vector3.forward * halfExtents.z + scaledDirection;

            Vector3[] corners = new Vector3[8];
            corners[0] = center - right - up - forward;
            corners[1] = center + right - up - forward;
            corners[2] = center - right + up - forward;
            corners[3] = center + right + up - forward;
            corners[4] = center - right - up + forward;
            corners[5] = center + right - up + forward;
            corners[6] = center - right + up + forward;
            corners[7] = center + right + up + forward;

            for (int i = 0; i < 4; i++)
            {
                Debug.DrawLine(corners[i], corners[(i + 1) % 4], color, duration); // Draw base rectangle
                Debug.DrawLine(corners[i + 4], corners[(i + 1) % 4 + 4], color, duration); // Draw top rectangle
                Debug.DrawLine(corners[i], corners[i + 4], color, duration); // Connect bases
            }
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

        #endregion
    }
}