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

        public int GetInt(string statusName)
        {
            if(statusIntDictionary.TryGetValue(statusName, out var statusValue)) {return statusValue;}
            return 0;
        }
        public float GetFloat(string statusName)
        {
            if(statusFloatDictionary.TryGetValue(statusName, out var statusValue)) {return statusValue;}
            return 0f;
        }
    }
}