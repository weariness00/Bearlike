using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using Util;

namespace Item
{
    public class ItemObjectList : Singleton<ItemObjectList>
    {
        #region Static

        public static ItemBase GetFromId(int id)
        {
            Instance.Init();
            
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

        public static ItemBase GetFromName(string itemName)
        {
            Instance.Init();
            
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

        public static int Length => Instance.itemIdArray.Length;
        public static int[] ItemIDArray() => Instance.itemIdArray;
        
        #endregion
        
        [SerializeField] private List<ItemBase> itemList = new List<ItemBase>();
        private int[] itemIdArray;
        private bool _isInit = false;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void Init()
        {
            if (_isInit == false)
            {
                itemIdArray = new int[itemList.Count];
                foreach (var item in itemList)
                {
                    item.info.SetJsonData(ItemBase.GetInfoData(item.Id));
                }

                itemIdArray = itemList.Select(item => item.Id).ToArray();
            }
        }

        public void SetList(List<ItemBase> list) => itemList = list;
        public List<ItemBase> GetList() => itemList;
    }
}

