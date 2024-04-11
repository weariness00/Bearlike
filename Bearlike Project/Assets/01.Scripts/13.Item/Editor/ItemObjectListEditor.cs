using System.Collections.Generic;
using Skill;
using UnityEditor;
using UnityEngine;

namespace Item.Editor
{
    [CustomEditor(typeof(ItemObjectList))]
    public class ItemObjectListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("아이템 리스트 불러오기"))
            {
                var script = target as ItemObjectList;

                var guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/02.Prefabs/13.Item" });
                List<ItemBase> itemList = new List<ItemBase>();
                
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if(gameObject.TryGetComponent(out ItemBase item)) 
                        itemList.Add(item);
                }

                script.itemList = itemList;
            }
        }
    }
}