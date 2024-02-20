using System;
using System.Collections.Generic;
using Fusion;
using Script.Util;
using UnityEngine;

namespace Item
{
    public class ItemObjectList : Util.Singleton<ItemObjectList>
    {
        [SerializeField] private List<GameObject> itemObjectList = new List<GameObject>();
        private Dictionary<DictionaryUtil.MultiKey<int,string>, GameObject> _itemObjectDictionary = new Dictionary<DictionaryUtil.MultiKey<int, string>, GameObject>();

        protected override void Awake()
        {
            base.Awake();
            Init();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Init()
        {
            for (var i = 0; i < itemObjectList.Count; i++)
            {
                var itemObject = itemObjectList[i];
                
                var itemBase = itemObject.GetComponent<ItemBase>();
                var keyPair  = new DictionaryUtil.MultiKey<int,string>(itemBase.id, itemBase.itemName);
                _itemObjectDictionary.Add(keyPair, itemObject);
            }
        }

        public static GameObject GetObject(int id)
        {
            if (Instance._itemObjectDictionary.TryGetValue(new DictionaryUtil.MultiKey<int, string>(id), out var value))
            {
                return value;
            }

            return null;
        }
        
        public static GameObject GetObject(string name)
        {
            if (Instance._itemObjectDictionary.TryGetValue(new DictionaryUtil.MultiKey<int, string>(name), out var value))
            {
                return value;
            }

            return null;
        }
    }
}

