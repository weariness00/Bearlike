using System.Collections.Generic;
using Newtonsoft.Json;

namespace Weapon.Bullet
{
    public class BulletJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Name")] public string Name;
        [JsonProperty("Explain")] public string Explain;
    }
}