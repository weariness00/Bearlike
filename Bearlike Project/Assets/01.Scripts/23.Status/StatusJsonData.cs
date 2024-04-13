using System.Collections.Generic;
using Newtonsoft.Json;

namespace Status
{
    public struct StatusJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Name")] public string Name;
        
        [JsonProperty("Status Int")] public Dictionary<string, int> statusIntDictionary;
        [JsonProperty("Status Float")] public Dictionary<string, float> statusFloatDictionary;

        public int GetInt(string statusName) => statusIntDictionary == null ? 0 : statusIntDictionary.TryGetValue(statusName, out var statusValue) ? statusValue : 0;
        public float GetFloat(string statusName) => statusFloatDictionary == null ? 0 : statusFloatDictionary.TryGetValue(statusName, out var statusValue) ? statusValue : 0f;
        public bool HasFloat(string statusName) => statusFloatDictionary != null && statusFloatDictionary.TryGetValue(statusName, out var value);
        public bool HasInt(string statusName) => statusIntDictionary != null && statusIntDictionary.TryGetValue(statusName, out var value);
    }
}