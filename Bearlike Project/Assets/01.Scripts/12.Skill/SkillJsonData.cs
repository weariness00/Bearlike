using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Skill
{
    public struct SkillJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Name")] public string Name;
        [JsonProperty("Explain")] public string Explain;
        [JsonProperty("Cool Time")] public float CoolTime;
        [JsonProperty("Type")] public SKillType Type;
    }
}