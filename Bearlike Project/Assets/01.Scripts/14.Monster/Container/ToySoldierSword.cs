using BehaviorTree.Base;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class ToySoldierSword : MonsterBase
    {
        public Transform weaponTransform;

        [Header("VFX")] 
        public VisualEffect prickVFX; // 찌르는 VFX

        #region BT Function

        public override INode InitBT()
        {
            var findTarget = new ActionNode(FindTarget);
            var loop = new SequenceNode(
                findTarget
            );
            return loop;
        }

        #endregion
    }
}