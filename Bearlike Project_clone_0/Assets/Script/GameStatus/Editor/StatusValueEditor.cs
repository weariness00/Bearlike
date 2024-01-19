using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script.GameStatus.Editor
{
    [CustomPropertyDrawer(typeof(StatusValue))]
    public class StatusValuePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty min;
        private SerializedProperty max;
        private SerializedProperty current;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            max = property.FindPropertyRelative("_max");
            min = property.FindPropertyRelative("_min");
            current = property.FindPropertyRelative("_current");
            
            Rect labelPosition = new Rect(position.x, position.y, position.width, position.height);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int interval = 150;
            var currentSliderPos = new Rect(position.x, position.y, interval, position.height);
            // float currentFloat = current.intValue;
            // EditorGUI.Slider(currentPos, currentFloat, min.intValue, max.intValue);
            // current.intValue = (int)currentFloat;
            current.intValue = (int)GUI.HorizontalSlider(currentSliderPos, current.intValue, min.intValue, max.intValue);
            
            var intFieldPos = new Rect(position.x - 40, position.y, 30, position.height);
            current.intValue = EditorGUI.IntField(intFieldPos, current.intValue);
                
            var rangeTextPos = new Rect(position.x + interval, position.y, position.width, position.height);
            EditorGUI.LabelField(rangeTextPos, $"{min.intValue} ~ {max.intValue}");

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        public int GetDigitCount(int number)
        {
            int digitCount = 1;
            if (number < 0) digitCount++;
            int integerPart = Mathf.Abs(number);
            while (integerPart >= 10)
            {
                integerPart /= 10;
                digitCount++;
            }

            return digitCount;
        }
    }
}