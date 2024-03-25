using System;
using System.Collections.Generic;
using System.IO;
using Manager;
using Newtonsoft.Json;
using UnityEngine;

namespace Item.Looting
{
    public class LootingTable : MonoBehaviour
    {
        public bool isDrop; // 드랍을 했는지

        [SerializeField] private LootingItem[] _dropItems = Array.Empty<LootingItem>();
        
        // 어떤 아이템을 드랍하게 될지 계산
        /// <summary>
        /// 
        /// </summary>
        public void CalLootingItem(LootingItem[] lit)
        {
            DebugManager.ToDo("랜덤한 확률로 랜덤한 갯수를 떨구게 할지도 정하기");
            
            List<LootingItem> dropItemList = new List<LootingItem>();
            foreach (var lootingItem in lit)
            {
                if(lootingItem.IsDrop() == false) continue;

                dropItemList.Add(lootingItem);
            }

            _dropItems = dropItemList.ToArray();
        }

        // 드랍해야될 아이템을 스폰
        public void SpawnDropItem()
        {
            foreach (var dropItem in _dropItems)
            {
                // 네트워크 객체이면 Runner를 통해 스폰
                if (dropItem.IsNetworkObject)
                {
                    DebugManager.ToDo("네트워크 객체 아이템 스폰 만들어주기");
                }
                // 일반 객체라면 클라이언트에게만 보이도로 스폰
                else
                {
                    var dropObjectPrefab = ItemObjectList.GetObject(dropItem.ItemID);
                    if (dropObjectPrefab == null)
                    {
                        DebugManager.LogError($"아이템이 리스트에 존재하지 않습니다. {dropItem.ItemName}, {dropItem.ItemID}");
                        continue;
                    }
                    
                    var item = Instantiate(dropObjectPrefab).GetComponent<ItemBase>();
                    item.transform.position = gameObject.transform.position;
                    item.Amount.Current = item.Amount.Max = dropItem.Amount;
                }
            }

            _dropItems = null;
            isDrop = true;
        }
    }
}

