using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(Enum), true)]
    public class EnumDrawer : PropertyDrawer
    {
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Enum targetEnum = GetBaseProperty<Enum>(property);

        string[] enumNames = Enum.GetNames(targetEnum.GetType());
        string[] displayNames = new string[enumNames.Length];

        for (int i = 0; i < enumNames.Length; i++)
        {
            FieldInfo fieldInfo = targetEnum.GetType().GetField(enumNames[i]);
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            displayNames[i] = attributes.Length > 0 ? attributes[0].Description : enumNames[i];
        }

        int index = EditorGUI.Popup(position, label.text, property.enumValueIndex, displayNames);
        if (index != property.enumValueIndex)
        {
            property.enumValueIndex = index;
        }
    }

    private T GetBaseProperty<T>(SerializedProperty prop)
    {
        // Separate the steps of finding the property path, which are divided by '.' 
        // E.g., "someArray.data[4].myField" would result in "someArray", "data[4]", "myField"
        string[] separatedPaths = prop.propertyPath.Split('.');
        object reflectionTarget = prop.serializedObject.targetObject as object;

        foreach (var path in separatedPaths)
        {
            if (path.Contains("Array"))
            {
                var arrayName = path.Substring(0, path.IndexOf('['));
                var index = Convert.ToInt32(path.Substring(path.IndexOf('[')).Replace("[", "").Replace("]", ""));
                reflectionTarget = GetValue_Imp(reflectionTarget, arrayName, index);
            }
            else
            {
                reflectionTarget = GetValue_Imp(reflectionTarget, path);
            }
        }

        return (T)reflectionTarget;
    }

    private object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;

        var type = source.GetType();
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (field == null)
        {
            var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
                return null;

            return property.GetValue(source, null);
        }

        return field.GetValue(source);
    }

    private object GetValue_Imp(object source, string arrayName, int index)
    {
        var enumerable = GetValue_Imp(source, arrayName) as System.Collections.IEnumerable;
        var enm = enumerable.GetEnumerator();

        // Move to the requested index
        for (int i = 0; i <= index; i++)
            enm.MoveNext();

        return enm.Current;
    }
    }
}