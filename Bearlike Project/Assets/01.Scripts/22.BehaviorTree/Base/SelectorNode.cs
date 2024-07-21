using System.Linq;
using Random = System.Random;

namespace BehaviorTree.Base
{
    /// <summary>
    /// Selector연산을 하는 노드(중간 노드) 왼쪽부터 오른쪽 순서로 연산
    /// </summary>
    public sealed class SelectorNode : INode
    {
        private INode.NodeState _state;
        private INode[] _children;
        private INode _checkPointChild;
        private bool _isRandom;

        public SelectorNode(bool isRandom = false, params INode[] children)
        {
            _children = children;
            _isRandom = isRandom;
        }

        public INode.NodeState Evaluate()
        {
            foreach (var child in _isRandom ? Shuffle(_children) : _children)
            {
                if (_checkPointChild != null)
                {
                    if (child != _checkPointChild)
                    {
                        continue;
                    }
                    _checkPointChild = null;
                    
                    if (child is Detector detector)
                    {
                        switch (detector.GetChild().Evaluate())
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
        
        private INode[] Shuffle(INode[] array)
        {
            Random rng = new Random();
            INode[] copyArray = array.ToArray();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (copyArray[n], copyArray[k]) = (copyArray[k], copyArray[n]);
            }

            return copyArray;
        }
    }
}