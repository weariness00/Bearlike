using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectUpdate;
using Script.Util;
using UnityEngine;
using Util;

namespace Item
{
    public class ItemObjectList : Singleton<ItemObjectList>
    {
        [SerializeField] private List<GameObject> itemObjectList = new List<GameObject>();
        private readonly Dictionary<int, GameObject> _itemObjectDictionary = new Dictionary<int, GameObject>();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            
            foreach (var itemObject in itemObjectList)
            {
                var itemBase = itemObject.GetComponent<ItemBase>();
                _itemObjectDictionary.Add(itemBase.Id, itemObject);
            }
        }

        public static GameObject GetObject(int id) => Instance._itemObjectDictionary.TryGetValue(id, out var value) ? value : null;
    }
}

