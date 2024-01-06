using System;
using UnityEditor;
using UnityEngine;

namespace Script.Manager.Editor
{
    [CustomEditor(typeof(DebugManager))]
    public class DebugManagerEditor : UnityEditor.Editor
    {
        #region Property

        private SerializedProperty IsAllDebug;
        private SerializedProperty IsLog;
        private SerializedProperty IsLogWaring;
        private SerializedProperty IsLogError;
        
        #endregion

        private void OnEnable()
        {
            IsAllDebug = serializedObject.FindProperty("isDebug");
            IsLog = serializedObject.FindProperty("log");
            IsLogWaring = serializedObject.FindProperty("logWaring");
            IsLogError = serializedObject.FindProperty("logError");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(IsAllDebug);
            if (IsAllDebug.boolValue)
            {
                EditorGUILayout.PropertyField(IsLog);
                EditorGUILayout.PropertyField(IsLogWaring);
                EditorGUILayout.PropertyField(IsLogError);
            }

            serializedObject.ApplyModifiedProperties(); // 이게 없으면 Property Update가 안됨   
        }
    }
}