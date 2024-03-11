using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
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
                switch (child.Evaluate())
                {
                    case INode.NodeState.Break:
                        return INode.NodeState.Break;
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