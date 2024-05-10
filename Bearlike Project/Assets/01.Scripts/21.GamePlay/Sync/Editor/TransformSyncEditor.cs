using UnityEditor;
using UnityEngine;

namespace GamePlay.Sync.Editor
{
    [CustomEditor(typeof(TransformSync))]
    public class TransformSyncEditor : UnityEditor.Editor
    {
        #region Property

        private SerializedProperty TargetTransform;
        private SerializedProperty IsPosition;
        private SerializedProperty IsRotate;
        private SerializedProperty IsRotateX;
        private SerializedProperty IsRotateY;
        private SerializedProperty IsRotateZ;
        private SerializedProperty IsScale;

        #endregion

        private void OnEnable()
        {
            TargetTransform = serializedObject.FindProperty("targetTransform");
            IsPosition = serializedObject.FindProperty("isPosition");
            IsRotate = serializedObject.FindProperty("isRotate");
            IsRotateX = serializedObject.FindProperty("isRotateX");
            IsRotateY = serializedObject.FindProperty("isRotateY");
            IsRotateZ = serializedObject.FindProperty("isRotateZ");
            IsScale = serializedObject.FindProperty("isScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(TargetTransform);
            EditorGUILayout.PropertyField(IsPosition);
            EditorGUILayout.PropertyField(IsRotate);
            if (IsRotate.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                // X 토글
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                rect.x = 100;
                rect.width = 40; // 라벨 너비 + 체크박스 너비
                IsRotateX.boolValue = EditorGUI.ToggleLeft(rect, "X", IsRotateX.boolValue);
        
                // Y 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsRotateY.boolValue = EditorGUI.ToggleLeft(rect, "Y", IsRotateY.boolValue);
        
                // Z 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsRotateZ.boolValue = EditorGUI.ToggleLeft(rect, "Z", IsRotateZ.boolValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.PropertyField(IsScale);

            serializedObject.ApplyModifiedProperties(); // 이게 없으면 Property Update가 안됨   
        }
    }
}