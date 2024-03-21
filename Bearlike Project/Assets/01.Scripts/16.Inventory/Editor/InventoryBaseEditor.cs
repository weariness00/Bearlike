using System.Collections.Generic;
using UnityEditor;
using Inventory;
using UnityEngine;

[CustomEditor(typeof(InventoryBase<,>), true)]
[CanEditMultipleObjects]
public class InventoryBaseEditor : Editor
{
    public Dictionary<Component,Component> inventoryDict;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기본 인스펙터 UI를 그립니다.
        var editorInterface = target as IInventoryEditor;
        editorInterface.SetItem(inventoryDict);
    }

    // Dict 기반으로 아이템과 그에 해당하는 UI 보여주기
    public void ShowInventory()
    {
        int itemCount = 0;
        foreach (var (item, handles) in inventoryDict)
        {
            EditorGUILayout.ObjectField($"Block {itemCount}", item, typeof(Component), true);
        }
    }
}