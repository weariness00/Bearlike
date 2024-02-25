using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Allocator = Unity.Collections.Allocator;

namespace BehaviorTree
{
    [RequireComponent(typeof(Animator))]
    public sealed class PiggyBankAI : MonoBehaviour
    {
        [Header("Movement")] 
        [SerializeField] private float movementSpeed = 1.0f;
        
        #region Property

        private Rigidbody _rb;
        private BehaviorTreeRunner _btRunner;
        private Animator _animator = null;

        #endregion

        #region Job

        private struct BTJob : IJob
        {
            // private NativeArray<BehaviorTreeRunner> _jobBTRuner;
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
            // btJob = new BTJob(_btRunner);
        }

        private void Update()
        {
            // var handle = btJob.Schedule();
            //
            // handle.Complete();  // 몇 프레임 뒤에 호출할까? 고민해보자
            _btRunner.Operator();
        }

        // private void LateUpdate()
        // {
        //     handle.Complete();
        // }

        INode SettingBT()
        {
            return new SequenceNode(
                new List<INode>()
                {
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckAttackAction),
                            new ActionNode(StartAttack),
                        }
                    ),
                    new ActionNode(WalkAround),
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

        #region Attack

        /// <summary>
        /// 돼지저금통이 공격을 하는중인지 판단하는 함수
        /// </summary>
        INode.NodeState CheckAttackAction()
        {
            if (IsAnimationRunning("piggy_attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        INode.NodeState StartAttack()
        {
            _animator.SetTrigger("tAttack");
            return INode.NodeState.Success;
        }

        // INode.NodeState 
        
        #endregion

        #region Patrol

        INode.NodeState LookAround()
        {
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #region Walk

        INode.NodeState WalkAround()
        {
            PiggyBankWalkRPC();
            return INode.NodeState.Success;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PiggyBankWalkRPC()
        {
            var rb = GetComponent<Rigidbody>();
            
            rb.AddForce(new Vector3(0, 0, -movementSpeed));
        }
        
        #endregion
    }
}
