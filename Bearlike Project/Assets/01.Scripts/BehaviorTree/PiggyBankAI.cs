using System.Collections.Generic;
using BehaviorTree.Base;
using State.StateClass;
using State.StateClass.Base;
using Unity.Collections;
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
        private StatusBase _status;

        #endregion
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _btRunner = new BehaviorTreeRunner(SettingBT());
            _status = GetComponent<MonsterStatus>();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            _btRunner.Operator();
        }

        INode SettingBT()
        {
            return new SelectorNode(
                new List<INode>()
                {
                    new ActionNode(WalkAround),
                    new SelectorNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckHp),
                        }
                    ),                    
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckAttackAction),
                            new ActionNode(CheckBoundery),
                            new ActionNode(StartAttack),
                        }
                    ),
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
        
        /// <summary>
        /// 돼지저금통이 이동하는 함수 ==> 추후에 정밀하게 이동할 예정
        /// </summary>
        INode.NodeState WalkAround()
        {
            _rb.velocity = new Vector3(0, 0, -movementSpeed);
            
            return INode.NodeState.Success;
        }
        
        #endregion

        #region 방어 OR 도주

        private struct CheckHpJob : IJob
        {
            public float Current;
            public float Max;
            public NativeArray<float> Result;
            
            public void Execute()
            {
                Result[0] = Current / Max;
            }
        }
        
        INode.NodeState CheckHp()
        {
            NativeArray<float> results = new NativeArray<float>(1, Allocator.TempJob);
            
            CheckHpJob Job = new CheckHpJob()
            {
                Current = _status.hp.Current,
                Max = _status.hp.Max,
                Result = results
            };
            
            JobHandle jobHandle = Job.Schedule();
            
            jobHandle.Complete();
            
            float result = results[0];
            results.Dispose();
            
            if (result > 0.5f) // 정밀한 검사 필요
            {
                return INode.NodeState.Success;
            }
            
            // if (_status.hp.Current / _status.hp.Max > 0.5f)
            // {
            //     return INode.NodeState.Success;
            // }
            
            return INode.NodeState.Failure;
        }
        
        // 방어하는 함수 구현
        
        // 도주하는 함수 구현

        #endregion
        
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

        INode.NodeState CheckBoundery()
        {
            return INode.NodeState.Failure;
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
        

    }
}
