using GamePlay.Stage;
using Newtonsoft.Json;

namespace GamePlay.Stage
{
    public struct StageJsonData
    {
        [JsonProperty("ID")] public int id;
        [JsonProperty("Type")] public StageType stageType;
        [JsonProperty("Title")]public string title;
        [JsonProperty("Explain")]public string explain;
    }
}