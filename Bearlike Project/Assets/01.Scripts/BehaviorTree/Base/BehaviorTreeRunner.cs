using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BehaviorTree.Base
{
    public struct BehaviorTreeRunner
    {
        private INode _rootNode;

        public BehaviorTreeRunner(INode rootNode) => _rootNode = rootNode;

        public INode.NodeState Operator()
        {
            return _rootNode.Evaluate();
        }
    }
}