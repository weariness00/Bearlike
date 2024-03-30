using Data;
using UnityEngine;

namespace GamePlay.Stage
{
    [System.Serializable]
    public struct StageInfo : IJsonData<StageJsonData>
    {
        public int id;
        public StageType stageType;
        public string title;
        public string explain;
        public Texture2D image;
        
        public StageJsonData GetJsonData()
        {
            return new StageJsonData();
        }

        public void SetJsonData(StageJsonData json)
        {
            stageType = json.stageType;
            title = json.title;
            explain = json.explain;
        }
    }
}