using System.Collections.Generic;
using UI.Inventory;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryBase<,>), true)]
[CanEditMultipleObjects]
public class InventoryBaseEditor : Editor
{
    public Dictionary<Component,Component> inventoryDict = new Dictionary<Component, Component>();
    public bool isFoldout;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기본 인스펙터 UI를 그립니다.
        var editorInterface = target as IInventoryEditor;
        editorInterface.SetItem(inventoryDict);
        ShowInventory();
    }

    // Dict 기반으로 아이템과 그에 해당하는 UI 보여주기
    public void ShowInventory()
    {
        int itemCount = 0;
        isFoldout = EditorGUILayout.Foldout(isFoldout, "Inventory Block List");
        if (isFoldout)
        {
            EditorGUILayout.BeginVertical();
            foreach (var (item, handles) in inventoryDict)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"\tBlock {itemCount}");
                EditorGUILayout.ObjectField(item, typeof(Component), true);
                EditorGUILayout.ObjectField(handles, typeof(Component), true);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}