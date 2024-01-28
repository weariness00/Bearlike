using UnityEngine;
using UnityEngine.Serialization;

namespace Item.Looting
{
    [System.Serializable]
    public struct LootingItem
    {
        public int ItemID;
        public string ItemName;
        public bool IsNetworkObject; // 모든 플레이어가 볼 수 있는 오브젝트인지

        public bool IsFixed; // 확정적으로 드랍인지
        public int Amount; // 몇개를 드랍할 것인지
        [Range(0f,100f)]public float Probability; // 몇퍼센트의 확률
        
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