
using Newtonsoft.Json;

namespace Weapon.Gun
{
    public struct GunJsonData
    {
        [JsonProperty("ID")] public int Id;
        [JsonProperty("Name")] public string Name;
        [JsonProperty("Explain")] public string Explain;
    }
}

