using System.Collections.Generic;
using UnityEngine;

namespace Item.Looting
{
    public class LootingTable : MonoBehaviour
    {
        public List<LootingItem> itemList;
        
        public LootingItem[] CalLootingItem()
        {
            List<LootingItem> dropItemList = new List<LootingItem>();
            foreach (var lootingItem in itemList)
            {
                if(lootingItem.IsDrop() == false) continue;

                dropItemList.Add(lootingItem);
            }
            
            return dropItemList.ToArray();
        }
    }
}

