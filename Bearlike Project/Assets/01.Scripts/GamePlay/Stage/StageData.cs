using UnityEngine;

namespace GamePlay.StageLevel
{
    [CreateAssetMenu(fileName = "Stage", menuName = "Stage/Make Stage Type", order = 0)]
    public class StageData : ScriptableObject
    {
        public SceneReference sceneReference;
        public StageLevelInfo info;
    }
}