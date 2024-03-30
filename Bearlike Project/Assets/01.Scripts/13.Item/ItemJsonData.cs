using System.Collections.Generic;
using Newtonsoft.Json;

namespace Item
{
    public struct ItemJsonData
    {
        [JsonProperty("ID")] public int id;
        [JsonProperty("Name")] public string name;
        [JsonProperty("Explain")] public string explain;
    }
}