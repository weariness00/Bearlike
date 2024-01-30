using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Script.Manager;
using UnityEngine;

namespace Item.Looting
{
    public class LootingTable : MonoBehaviour
    {
        public List<LootingItem> itemList;
        public bool isDrop; // 드랍을 했는지

        [SerializeField] private LootingItem[] _dropItems;

        private void Start()
        {
            CalLootingItem();
            var path = Application.dataPath + $"/Json/Looting Table/Monster Looting Table List.json";
            var data = File.ReadAllText(path);

            _dropItems = JsonConvert.DeserializeObject<LootingItem[]>(data); 

        }

        private void OnDestroy()
        {
            if (!isDrop) { SpawnDropItem();}
        }

        // 어떤 아이템을 드랍하게 될지 계산
        LootingItem[] CalLootingItem()
        {
            List<LootingItem> dropItemList = new List<LootingItem>();
            foreach (var lootingItem in itemList)
            {
                if(lootingItem.IsDrop() == false) continue;

                dropItemList.Add(lootingItem);
            }
            itemList.Clear();
            
            return dropItemList.ToArray();
        }

        // 드랍해야될 아이템을 스폰
        void SpawnDropItem()
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
                    
                    var obj = Instantiate(dropObjectPrefab);
                    obj.transform.position = gameObject.transform.position;
                }
            }

            _dropItems = null;
            isDrop = true;
        }
    }
}

