using System.Collections.Generic;
using Newtonsoft.Json;

namespace Weapon.Gun
{
    public struct GunJsonData
    {
        [JsonProperty("GunID")] public int Id;
        [JsonProperty("Status Name")] public string Name;
        
        [JsonProperty("Status Int")] public Dictionary<string, int> statusIntDictionary;
        [JsonProperty("Status Float")] public Dictionary<string, float> statusFloatDictionary;

        public int GetInt(string statusName) => statusIntDictionary == null ? 0 : statusIntDictionary.TryGetValue(statusName, out var statusValue) ? statusValue : 0;
        public float GetFloat(string statusName) => statusFloatDictionary == null ? 0 : statusFloatDictionary.TryGetValue(statusName, out var statusValue) ? statusValue : 0f;

    }
}

