using Newtonsoft.Json;

namespace Monster
{
    public struct MonsterJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Name")] public string Name;
        [JsonProperty("Type")] public string Type;
        [JsonProperty("Explain")] public string Explain;
    }
}