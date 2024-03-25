﻿using Data;
using Inventory;
using Status;
using UnityEngine;

namespace Item
{
    [System.Serializable]
    public struct ItemInfo : IJsonData<ItemJsonData>
    {
        public int id;
        public string itemName;
        
        public Texture2D icon; // 아이템 이미지

        public StatusValue<int> amount; // 아이템 총 갯수
        public string explain; // 아이템 설명

        public static implicit operator string(ItemInfo value)
        {
            return value.itemName;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ItemInfo info = (ItemInfo)obj;

            return id == info.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        
        #region Json 

        public ItemJsonData GetJsonData()
        {
            ItemJsonData json = new ItemJsonData();
            json.name = "Item";
            json.amount = amount;
            json.explain = explain;
            return json;
        }

        public void SetJsonData(ItemJsonData json)
        {
            amount.Current = json.amount;
            explain = json.explain;
        }

        #endregion
    }
}