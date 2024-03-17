using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Item.Looting
{
    [System.Serializable]
    public struct LootingItem
    {
        [JsonProperty("Target ID")]public int TargetObjectID; // Monster가 될 수 있고 상자가 될 수 있고 아이템이 될 수도 있다.
        [JsonProperty("Item ID")]public int ItemID;
        [JsonProperty("Item Name")]public string ItemName;
        
        [JsonProperty("Probability")][Range(0f,100f)]public float Probability; // 몇퍼센트의 확률
        [JsonProperty("Amount")]public int Amount; // 몇개를 드랍할 것인지
        [JsonProperty("Is Fixed")]public bool IsFixed; // 확정적으로 드랍인지

        [JsonProperty("Is Networked")]public bool IsNetworkObject; // 모든 플레이어가 볼 수 있는 오브젝트인지
        
        public bool IsDrop()
        {
            if (IsFixed) { return true;}
            if (Probability > Random.Range(0f, 100f))
            {
                return true;
            }
            return false;
        }
    }
}