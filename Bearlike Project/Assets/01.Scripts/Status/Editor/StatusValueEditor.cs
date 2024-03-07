using Codice.CM.Common;
using Status;
using UnityEditor;
using UnityEngine;

namespace Scripts.State.GameStatus.Editor
{
    [CustomPropertyDrawer(typeof(StatusValue<float>))]
    public class StatusValueFloatPropertyDrawer : PropertyDrawer
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

            var intFieldPos = new Rect(position.x - 40, position.y, 30, position.height);
            current.floatValue = EditorGUI.FloatField(intFieldPos, current.floatValue);
            if (current.floatValue < min.floatValue)
            {
                current.floatValue = min.floatValue;
            }
            else if (current.floatValue > max.floatValue)
            {
                current.floatValue = max.floatValue;
            }
            
            int sumInterval = 0;

            int sliderInterval = 150;
            var currentSliderPos = new Rect(position.x, position.y, sliderInterval, position.height);
            current.floatValue = (int)GUI.HorizontalSlider(currentSliderPos, current.floatValue, min.floatValue, max.floatValue);
            sumInterval += sliderInterval + 10;
                
            // TO DO
            // Value Inetrval을 수치에 따라 동적으로 바꿀 수 있게 하기
            int valueInterval = 60;
            var minPos = new Rect(position.x + sumInterval, position.y, valueInterval, position.height);
            min.floatValue = EditorGUI.FloatField(minPos, min.floatValue);
            sumInterval += valueInterval;
                
            int textInterval = 20;
            var rangeTextPos = new Rect(position.x + sumInterval, position.y, textInterval, position.height);
            EditorGUI.LabelField(rangeTextPos, $" ~ ");
            sumInterval += textInterval;
                
            var maxPos = new Rect(position.x + sumInterval, position.y, valueInterval, position.height);
            max.floatValue = EditorGUI.FloatField(maxPos, max.floatValue);
            sumInterval += valueInterval;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        [CustomPropertyDrawer(typeof(StatusValue<int>))]
        public class StatusValueIntegerPropertyDrawer : PropertyDrawer
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

                var intFieldPos = new Rect(position.x - 40, position.y, 30, position.height);
                current.intValue = EditorGUI.IntField(intFieldPos, current.intValue);
                if (current.intValue < min.intValue)
                {
                    current.intValue = min.intValue;
                }
                else if (current.intValue > max.intValue)
                {
                    current.intValue = max.intValue;
                }

                int sumInterval = 0;

                int sliderInterval = 150;
                var currentSliderPos = new Rect(position.x + sumInterval, position.y, sliderInterval, position.height);
                current.intValue = (int)GUI.HorizontalSlider(currentSliderPos, current.intValue, min.intValue, max.intValue);
                sumInterval += sliderInterval + 10;
                
                // TO DO
                // Value Inetrval을 수치에 따라 동적으로 바꿀 수 있게 하기
                int valueInterval = 60;
                var minPos = new Rect(position.x + sumInterval, position.y, valueInterval, position.height);
                min.intValue =EditorGUI.IntField(minPos, min.intValue);
                sumInterval += valueInterval;
                
                int textInterval = 20;
                var rangeTextPos = new Rect(position.x + sumInterval, position.y, textInterval, position.height);
                EditorGUI.LabelField(rangeTextPos, $" ~ ");
                sumInterval += textInterval;
                
                var maxPos = new Rect(position.x + sumInterval, position.y, valueInterval, position.height);
                max.intValue = EditorGUI.IntField(maxPos, max.intValue);
                sumInterval += valueInterval;
                
                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();

                property.serializedObject.ApplyModifiedProperties();
            }
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
