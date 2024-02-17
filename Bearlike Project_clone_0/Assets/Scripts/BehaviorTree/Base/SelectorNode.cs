using System.Collections.Generic;

namespace BehaviorTree.Base
{
    /// <summary>
    /// Selector연산을 하는 노드(중간 노드) 왼쪽부터 오른쪽 순서로 연산
    /// </summary>
    public sealed class SelectorNode : INode
    {
        private List<INode> _childs;

        public SelectorNode(List<INode> childs) => _childs = childs;
        
        public INode.NodeState Evaluate()
        {
            if (_childs == null)
            {
                return INode.NodeState.Failure;
            }

            foreach (var child in _childs)
            {
                switch (child.Evaluate())
                {
                    case INode.NodeState.Running:
                        return INode.NodeState.Running;
                    case INode.NodeState.Success:
                        return INode.NodeState.Success;
                    // 생략
                    // case INode.NodeState.Failure:
                    //     continue;
                }
            }

            return INode.NodeState.Failure;
        }
    }
}