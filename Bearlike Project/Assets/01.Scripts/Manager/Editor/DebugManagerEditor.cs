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
        private SerializedProperty IsToDo;
        private SerializedProperty IsDrawRay;
        
        #endregion

        private void OnEnable()
        {
            IsAllDebug = serializedObject.FindProperty("isDebug");
            IsLog = serializedObject.FindProperty("log");
            IsLogWaring = serializedObject.FindProperty("logWaring");
            IsLogError = serializedObject.FindProperty("logError");
            IsToDo = serializedObject.FindProperty("isToDo");
            IsDrawRay = serializedObject.FindProperty("drawRay");
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
                EditorGUILayout.PropertyField(IsToDo);
                EditorGUILayout.PropertyField(IsDrawRay);
            }

            serializedObject.ApplyModifiedProperties(); // 이게 없으면 Property Update가 안됨   
        }
    }
}