using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Base
{
    /// <summary>
    /// Sequence연산을 하는 노드(중간 노드) 왼쪽부터 오른쪽 순서로 연산
    /// </summary>
    public sealed class SequenceNode : INode
    {
        private List<INode> _childs;
        private INode _checkPointChild = null;
        
        public SequenceNode(List<INode> childs) => _childs = childs;
        public SequenceNode(params INode[] children) => _childs = children.ToList();
        
        public INode.NodeState Evaluate()   
        {
            if (_childs == null || _childs.Count == 0)
            {
                return INode.NodeState.Failure;
            }

            foreach (var child in _childs)
            {
                if (_checkPointChild != null)
                {
                    if (child != _checkPointChild)
                    {
                        continue;
                    }
                    _checkPointChild = null;
                }
                switch (child.Evaluate())
                {
                    case INode.NodeState.Running:
                        _checkPointChild = child;
                        return INode.NodeState.Running;
                    case INode.NodeState.Success:
                        continue;
                    case INode.NodeState.Failure:
                        return INode.NodeState.Failure;
                }
            }

            return INode.NodeState.Success;
        }
    }
}