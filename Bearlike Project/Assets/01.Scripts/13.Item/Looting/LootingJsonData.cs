using Newtonsoft.Json;

namespace Item.Looting
{
    public struct LootingJsonData
    {
        [JsonProperty("ID")] public int TargetID;
        [JsonProperty("LootingTable")] public LootingItem[] LootingItems;
    }
}