using System.Collections.Generic;
using Manager;
using Util;

namespace Item
{
    public class ItemObjectList : Singleton<ItemObjectList>
    {
        public List<ItemBase> itemList = new List<ItemBase>();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            
        }

        public static ItemBase GetFromId(int id)
        {
            foreach (var item in Instance.itemList)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            
            DebugManager.LogError($"ID : {id} 인 아이템이 존재하지 않습니다.");
            
            return null;
        }

        public static ItemBase GetObject(string itemName)
        {
            foreach (var item in Instance.itemList)
            {
                if (item.Name == itemName)
                {
                    return item;
                }
            }
            
            DebugManager.LogError($"Name : {itemName} 인 아이템이 존재하지 않습니다.");
            
            return null;
        }
    }
}

