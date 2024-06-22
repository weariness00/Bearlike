using BehaviorTree.Base;
using Sound;
using UnityEngine.Serialization;

namespace Monster.Container
{
    public class BoxJester : MonsterBase
    {
        private BehaviorTreeRunner _behaviorTreeRunner;

        public SoundBox soundBox;

        #region Unity Event Function

        void Awake()
        {
            
        }

        public override INode InitBT()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Member Function

        

        #endregion

        #region BT Function

        

        #endregion
    }
}