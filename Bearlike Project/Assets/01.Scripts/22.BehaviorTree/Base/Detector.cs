using System;

namespace BehaviorTree.Base
{
    public class Detector : INode
    {
        private INode _child;
        private Func<bool> _detectFunc;

        public Detector(Func<bool> func, INode child)
        {
            _detectFunc = func;
            _child = child;
        }
        
        public INode.NodeState Evaluate()
        {
            var isValue = _detectFunc?.Invoke() ?? false;
            if (isValue)
            {
                return _child.Evaluate();
            }
            else
            {
                return INode.NodeState.Failure;
            }
        }
    }
}