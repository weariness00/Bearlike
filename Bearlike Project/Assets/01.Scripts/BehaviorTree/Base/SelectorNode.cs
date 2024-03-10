using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

namespace BehaviorTree.Base
{
    /// <summary>
    /// Selector연산을 하는 노드(중간 노드) 왼쪽부터 오른쪽 순서로 연산
    /// </summary>
    public sealed class SelectorNode : INode
    {
        private INode.NodeState _state;
        private List<INode> _childs = new List<INode>();
        private INode _checkPointChild = null;

        public SelectorNode(List<INode> childs) => _childs = childs;
        public SelectorNode(params INode[] children) => _childs = children.ToList();

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