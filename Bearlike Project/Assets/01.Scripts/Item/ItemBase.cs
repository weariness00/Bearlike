using System;
using System.IO;
using Newtonsoft.Json;
using Script.Data;
using Status;
using UnityEngine;

namespace Item
{
    public class ItemBase : MonoBehaviour, IJsonData<ItemJsonData>
    {
        public static string path = $"{Application.dataPath}/Json/Item/";

        public int id;
        public string itemName;
        
        public Sprite icon; // 아이템 이미지

        public StatusValue<int> amount; // 아이템 총 갯수
        public string explain; // 아이템 설명

        #region Static Method

        public static bool SaveJsonData(ItemJsonData json) => IJsonData<ItemJsonData>.SaveJsonData(json, json.name, path);

        #endregion

        public virtual void GetItem<T>(T target)
        {
            
        }

        #region JsonData Interface

        public virtual ItemJsonData GetJsonData()
        {
            ItemJsonData json = new ItemJsonData();
            json.name = "Item";
            json.amount = amount;
            json.explain = explain;
            return json;
        }

        public virtual void SetJsonData(ItemJsonData json)
        {
            amount.Current = json.amount;
            explain = json.explain;
        }

        #endregion
    }
}

