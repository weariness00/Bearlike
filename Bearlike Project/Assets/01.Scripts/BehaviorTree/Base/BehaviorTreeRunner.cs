using System.Threading;
using Unity.Collections;
using Unity.Jobs;

namespace BehaviorTree.Base
{
    public struct BehaviorTreeRunner
    {
        private INode _rootNode;

        public BehaviorTreeRunner(INode rootNode) => _rootNode = rootNode;

        public void Operator()
        {
            _rootNode.Evaluate();
        }
    }
}