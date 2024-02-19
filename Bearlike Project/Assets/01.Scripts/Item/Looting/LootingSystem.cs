using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectUpdate;
using Script.Manager;
using UnityEngine;
using Util;

namespace Item.Looting
{
    public class LootingSystem : Singleton<LootingSystem>
    {
        [HideInInspector] public Dictionary<int, LootingItem[]> monsterLootingItemDictionary = new Dictionary<int, LootingItem[]>();
        [HideInInspector] public Dictionary<int, LootingItem[]> stageLootingItemDictionary = new Dictionary<int, LootingItem[]>();

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
            int id = 0;
            int currentArrayIndex = 0;
            for (int i = 0; i < lootingItems.Length; i++)
            {
                if (lootingItems[i].TargetObjectID == id) {continue;}

                int subArrayLength = i - currentArrayIndex;
                LootingItem[] subLootingItems = new LootingItem[subArrayLength];
                Array.Copy(lootingItems, currentArrayIndex, subLootingItems, 0, subArrayLength);
                lootingItemDict.Add(id,subLootingItems);

                id = lootingItems[i].TargetObjectID;
                currentArrayIndex = i;
            }

            { // 마지막 배열도 분해하기
                int subArrayLength = lootingItems.Length - currentArrayIndex;
                LootingItem[] subLootingItems = new LootingItem[subArrayLength];
                Array.Copy(lootingItems, currentArrayIndex, subLootingItems, 0, subArrayLength);
                lootingItemDict.Add(id,subLootingItems);
            }
        }
    }
}

