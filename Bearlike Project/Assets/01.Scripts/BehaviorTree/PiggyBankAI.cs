using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using Fusion;
using Unity.Jobs;
using UnityEngine;

namespace BehaviorTree
{
    [RequireComponent(typeof(Animator))]
    public sealed class PiggyBankAI : NetworkBehaviour
    {
        [Header("Movement")] 
        [SerializeField] private float movementSpeed = 5.0f;
        
        #region Property

        private Rigidbody _rb;
        private BehaviorTreeRunner _btRunner = null;
        private Animator _animator = null;

        #endregion

        #region Job

        private struct BTJob : IJob
        {
            private BehaviorTreeRunner _jobBTRuner;

            public BTJob(BehaviorTreeRunner btRunner)
            {
                _jobBTRuner = btRunner;
            }
            
            public void Execute()
            {
                _jobBTRuner.Operator();
            }
        }

        private BTJob btJob;
        #endregion
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _btRunner = new BehaviorTreeRunner(SettingBT());
        }

        private void Start()
        {
            btJob = new BTJob(_btRunner);
        }

        private void Update()
        {
            var handle = btJob.Schedule();

            handle.Complete();
        }

        INode SettingBT()
        {
            return new SelectorNode(
                new List<INode>()
                {
                    
                    // 마지막 행동
                    new ActionNode(WalkAround)
                }
            );
        }
        
        bool IsAnimationRunning(string stateName)
        {
            if (_animator != null)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    var normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    return normalizedTime != 0 && normalizedTime < 1f;
                }
            }
            
            return false;
        }

        #region Walk

        INode.NodeState WalkAround()
        {
            
            return INode.NodeState.Failure;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PiggyBankWalkRPC()
        {
            var rb = GetComponent<Rigidbody>();
        }
        
        #endregion
    }
}
