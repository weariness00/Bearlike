using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Skill;
using UnityEditor;
using UnityEngine;
using Util;

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
                
                JsonConvertExtension.Load("Item", (json) =>
                {
                    var data = JsonConvert.DeserializeObject<ItemJsonData[]>(json);
                    
                    foreach (var itemBase in itemList)
                    {
                        itemBase.info.SetJsonData( data.FirstOrDefault(i => i.id == itemBase.Id));
                    }
                });


                script.itemList = itemList;
            }
        }
    }
}