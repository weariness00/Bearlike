using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Item.Looting
{
    [System.Serializable]
    public struct LootingItem
    {
        [JsonProperty("Item ID")]public int ItemID;
        [JsonProperty("Probability")][Range(0f,100f)]public float Probability; // 몇퍼센트의 확률
        [JsonProperty("Amount")]public int Amount; // 몇개를 드랍할 것인지
        [JsonProperty("Is Networked")]public bool IsNetworkObject; // 모든 플레이어가 볼 수 있는 오브젝트인지
        
        public bool IsDrop()
        {
            if (Random.Range(0f, 1f) <= Probability)
            {
                return true;
            }
            return false;
        }
    }
}