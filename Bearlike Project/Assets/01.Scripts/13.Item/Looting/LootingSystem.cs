using System;
using System.Collections.Generic;
using Manager;
using Newtonsoft.Json;
using ProjectUpdate;
using UnityEngine;
using Util;

namespace Item.Looting
{
    public class LootingSystem : Singleton<LootingSystem>
    {
        [HideInInspector] public Dictionary<int, LootingItem[]> monsterLootingItemDictionary = new Dictionary<int, LootingItem[]>();
        [HideInInspector] public Dictionary<int, LootingItem[]> stageLootingItemDictionary = new Dictionary<int, LootingItem[]>();

        #region Static Function

        public static LootingItem[] MonsterTable(int id)
        {
            return Instance.monsterLootingItemDictionary.TryGetValue(id, out var table) ? table : Array.Empty<LootingItem>();
        }

        #endregion
        
        protected override void Awake()
        {
            JsonConvertExtension.Load(ProjectUpdateManager.Instance.monsterLootingTableList,
                (data) =>
                {
                    SetLootingTable(JsonConvert.DeserializeObject<LootingItem[]>(data), monsterLootingItemDictionary);
                    DebugManager.Log("Monster Looting Table List를 불러왔습니다.");
                }
            );
            
            JsonConvertExtension.Load(ProjectUpdateManager.Instance.stageLootingTableList,
                (data) =>
                {
                    SetLootingTable(JsonConvert.DeserializeObject<LootingItem[]>(data), stageLootingItemDictionary);
                    DebugManager.Log("Stage Looting Table List를 불러왔습니다.");
                }
            );
        }

        private void SetLootingTable(LootingItem[] lootingItems, Dictionary<int, LootingItem[]> lootingItemDict)
        {
            // Target ID에 따라 Looting Table에 추가하기
            Dictionary<int, List<LootingItem>> targetTables = new  Dictionary<int, List<LootingItem>>();
            foreach (var item in lootingItems)
            {
                if (targetTables.TryGetValue(item.TargetObjectID, out var table))
                {
                    table.Add(item);
                }
                else
                {
                    targetTables.Add(item.TargetObjectID, new List<LootingItem>(){item});
                }
            }
            
            foreach (var (id, table) in targetTables)
            {
                lootingItemDict.Add(id, table.ToArray());
                table.Clear();
            }
            
            targetTables.Clear();
        }
    }
}

