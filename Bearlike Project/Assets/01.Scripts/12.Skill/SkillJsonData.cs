using System.Collections.Generic;
using Newtonsoft.Json;

namespace Skill
{
    public class SkillJsonData
    {
        [JsonProperty("Explain")] public string explain;
        [JsonProperty("CoolTime")] public float coolTime;

        [JsonProperty("Status Int")] public Dictionary<string, int> statusIntDictionary;
        [JsonProperty("Status Float")] public Dictionary<string, float> statusFloatDictionary;
        
        public int GetStatusInt(string statusName)
        {
            if (statusIntDictionary.TryGetValue(statusName, out int value))
            {
                return value;
            }

            return 0;
        }

        public float GetStatusFloat(string statusName)
        {
            if (statusFloatDictionary.TryGetValue(statusName, out float value))
            {
                return value;
            }

            return 0f;
        }
    }
}