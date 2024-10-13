using UnityEditor;
using UnityEngine;

namespace GamePlay.Sync.Editor
{
    [CustomEditor(typeof(TransformSync))]
    public class TransformSyncEditor : UnityEditor.Editor
    {
        #region Property

        private SerializedProperty TargetTransform;

        private SerializedProperty PositionOffset;
        private SerializedProperty PositionMultiple;
        
        private SerializedProperty IsPosition;
        private SerializedProperty IsPositionLocal;
        private SerializedProperty IsPositionX, IsPositionY, IsPositionZ;
        private SerializedProperty IsRotate;
        private SerializedProperty IsRotateLocal;
        private SerializedProperty IsRotateX, IsRotateY, IsRotateZ;
        private SerializedProperty IsScale;

        #endregion

        private void OnEnable()
        {
            TargetTransform = serializedObject.FindProperty("targetTransform");
            
            PositionOffset = serializedObject.FindProperty("positionOffset");
            PositionMultiple = serializedObject.FindProperty("positionMultiple");
            
            IsPosition = serializedObject.FindProperty("isPosition");
            IsPositionLocal = serializedObject.FindProperty("isPositionLocal");
            IsPositionX = serializedObject.FindProperty("isPositionX");
            IsPositionY = serializedObject.FindProperty("isPositionY");
            IsPositionZ = serializedObject.FindProperty("isPositionZ");
            
            IsRotate = serializedObject.FindProperty("isRotate");
            IsRotateLocal = serializedObject.FindProperty("isRotateLocal");
            IsRotateX = serializedObject.FindProperty("isRotateX");
            IsRotateY = serializedObject.FindProperty("isRotateY");
            IsRotateZ = serializedObject.FindProperty("isRotateZ");
            
            IsScale = serializedObject.FindProperty("isScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(TargetTransform);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(PositionOffset);
            EditorGUILayout.PropertyField(PositionMultiple);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(IsPosition);
            if (IsPosition.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                // X 토글
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                rect.width = 50; // 라벨 너비 + 체크박스 너비
                rect.x = 40;
                IsPositionLocal.boolValue = EditorGUI.ToggleLeft(rect, "Local", IsPositionLocal.boolValue);

                rect.width = 40; // 라벨 너비 + 체크박스 너비
                rect.x += 30 + rect.width; // 라벨 너비 + 체크박스 너비
                IsPositionX.boolValue = EditorGUI.ToggleLeft(rect, "X", IsPositionX.boolValue);
        
                // Y 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsPositionY.boolValue = EditorGUI.ToggleLeft(rect, "Y", IsPositionY.boolValue);
        
                // Z 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsPositionZ.boolValue = EditorGUI.ToggleLeft(rect, "Z", IsPositionZ.boolValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(IsRotate);
            if (IsRotate.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                // X 토글
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                rect.width = 50; // 라벨 너비 + 체크박스 너비
                rect.x = 40;
                IsRotateLocal.boolValue = EditorGUI.ToggleLeft(rect, "Local", IsRotateLocal.boolValue);
                
                rect.width = 40; // 라벨 너비 + 체크박스 너비
                rect.x += 30 + rect.width; // 라벨 너비 + 체크박스 너비
                IsRotateX.boolValue = EditorGUI.ToggleLeft(rect, "X", IsRotateX.boolValue);
        
                // Y 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsRotateY.boolValue = EditorGUI.ToggleLeft(rect, "Y", IsRotateY.boolValue);
        
                // Z 토글
                rect.x += rect.width; // 이전 필드의 너비만큼 위치 조정
                IsRotateZ.boolValue = EditorGUI.ToggleLeft(rect, "Z", IsRotateZ.boolValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(IsScale);

            serializedObject.ApplyModifiedProperties(); // 이게 없으면 Property Update가 안됨   
        }
    }
}