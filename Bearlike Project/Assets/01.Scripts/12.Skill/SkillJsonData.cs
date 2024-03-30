using System.Collections.Generic;
using Newtonsoft.Json;

namespace Skill
{
    public struct SkillJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Explain")] public string Explain;
        [JsonProperty("CoolTime")] public float CoolTime;
        [JsonProperty("Type")] public SKillType Type;
    }
}