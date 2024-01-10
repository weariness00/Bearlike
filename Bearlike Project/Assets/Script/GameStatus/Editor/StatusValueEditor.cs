using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script.GameStatus.Editor
{
    [CustomPropertyDrawer(typeof(StatusValue))]
    public class StatusValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty min = property.FindPropertyRelative("_min");
            SerializedProperty max = property.FindPropertyRelative("_max");
            SerializedProperty current = property.FindPropertyRelative("_current");

            Rect labelPosition = new Rect(position.x, position.y, position.width, position.height);
            position = EditorGUI.PrefixLabel(
                labelPosition,
                EditorGUIUtility.GetControlID(FocusType.Passive),
                label
            );

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int interval = 150;
            var currentPos = new Rect(position.x, position.y, interval, position.height);
            EditorGUI.Slider(currentPos, current, min.intValue, max.intValue, GUIContent.none);
            
            var rangeTextPos = new Rect(position.x + interval, position.y, position.width, position.height);
            EditorGUI.LabelField(rangeTextPos, $"{min.intValue} ~ {max.intValue}");

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
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