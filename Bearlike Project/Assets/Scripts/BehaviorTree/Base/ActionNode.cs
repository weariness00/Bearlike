using System;

namespace BehaviorTree.Base
{
    /// <summary>
    /// 실제로 행동을 하는 노드(리프 노드에 위치)
    /// </summary>
    public sealed class ActionNode : INode
    {
        // Func Delegate를 통해서 행동을 실행 (반환값이 들어가는 함수들을 넣을것이기에 Func를 사용)
        private Func<INode.NodeState> _onUpdate = null;

        public ActionNode(Func<INode.NodeState> onUpdate)
        {
            _onUpdate = onUpdate;
        }

        public INode.NodeState Evaluate() => _onUpdate?.Invoke() ?? INode.NodeState.Failure;
    }
}