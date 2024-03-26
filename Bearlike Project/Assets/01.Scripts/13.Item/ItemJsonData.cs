using System.Collections.Generic;
using Newtonsoft.Json;
using Status;

namespace Item
{
    public class ItemJsonData
    {
        [JsonProperty("ID")]public int id;
        [JsonProperty("Name")]public string name;
        [JsonProperty("Explain")]public string explain;

        [JsonProperty("Status Int")]public Dictionary<string, int> statusIntDictionary = new Dictionary<string, int>();
        [JsonProperty("Status Float")]public Dictionary<string, float> statusFloatDictionary = new Dictionary<string, float>();

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