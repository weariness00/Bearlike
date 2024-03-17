using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Stage
{
    [System.Serializable]
    public struct StageInfo
    {
        [FormerlySerializedAs("StageLevelType")] public StageType stageType;
        public string title;
        public string explain;
        public Texture2D image;
    }
}