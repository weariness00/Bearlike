using Scripts.State.GameStatus;

namespace GamePlay.StageLevel
{
    [System.Serializable]
    public struct StageLevelInfo
    {
        public StageLevelType StageLevelType;
        public StatusValue<int> AliveMonsterCount;
    }
}