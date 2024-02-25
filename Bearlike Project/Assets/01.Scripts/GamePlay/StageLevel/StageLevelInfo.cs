using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;

namespace GamePlay.StageLevel
{
    [System.Serializable]
    public struct StageLevelInfo
    {
        public StageLevelType StageLevelType;
        public string title;
        public string explain;
    }
}