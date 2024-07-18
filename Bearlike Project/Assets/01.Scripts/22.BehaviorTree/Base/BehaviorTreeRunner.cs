using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BehaviorTree.Base
{
    public struct BehaviorTreeRunner
    {
        private readonly INode RootNode;

        public BehaviorTreeRunner(INode rootNode) => RootNode = rootNode;

        public INode.NodeState Operator()
        {
            return RootNode.Evaluate();
        }
    }
}