using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using Data;
using Manager;
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
        
        #region Component
        
        private Rigidbody _rb;
        private BehaviorTreeRunner _btRunner;
        private Animator _animator = null;
        private StatusBase _status;

        private GameManager _gameManager;
        private UserData _userData;
        private List<GameObject> _playerPrefabs;

        //[field:SerializeField] // 프로퍼티도 인스펙터에서 보여줌

        #endregion

        #region 속성
        
        private float _playerCount;
        
        private float _detectingRange;  // 돌진 감지 범위
        
        private static readonly int IsWalk = Animator.StringToHash("IsWalk");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int IsRest = Animator.StringToHash("IsRest");
        private static readonly int AttackType = Animator.StringToHash("Attack_Blend");

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
            _gameManager = GameManager.Instance;
            _playerCount = _gameManager.AlivePlayerCount;

            // TODO : foreach문에서 조건문을 계속 호출해서 성능 저하가 일어나는지 테스트 필요
            var players = GameObject.FindGameObjectsWithTag("Player");
            var index = 0;
            
            foreach (var player in players)
            {
                _playerPrefabs[index++] = player;
            }
            
            // _userData = UserData.Instance;
            // foreach (var userData in _userData.UserDictionary)
            // {
            //     // _playerPrefabs[userData.Value.ClientNumber] = userData.Value.PrefabRef;
            // }
            
            for (var i = 0; i < (int)_playerCount; ++i)
            {
                // 플레이어 고유번호 가지고 있어야함
                // _playerPrefabs[i] = 
            }
            
            _detectingRange = 10;
        }

        private void Update()
        {
            _btRunner.Operator();
        }

        INode SettingBT()
        {
            return new SequenceNode
            (
                new List<INode>()
                {
                    new ActionNode(WalkAround),     // Walk
                    new SelectorNode
                    (
                        new List<INode>()
                        {
                            new SequenceNode
                            (
                            new List<INode>()
                            {
                                new ActionNode(CheckMoreHp),        // Deffence
                                new ActionNode(CheckDeffenceAction),
                                new ActionNode(StartDeffence),
                            }
                            ),
                            new SequenceNode
                            (
                            new List<INode>()
                            {        
                                new ActionNode(CheckRunAction),        // Run
                                new ActionNode(StartRun),
                            }
                            )
                        }
                    ),                    
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckAttackAction),      // Kick
                            new ActionNode(CheckBoundery),
                            new ActionNode(StartAttack),
                        }
                    ),
                    new SelectorNode
                    (
                        new List<INode>()
                        {
                                        // Rush
                                        
                                        // JumpAttack
                                        
                                        // fart
                        }
                    ),
                                        // take a rest
                                        // sleep
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
            _animator.SetBool(IsWalk, true);
            _rb.velocity = new Vector3(0, 0, -movementSpeed);
            
            return INode.NodeState.Success;
        }
        
        #endregion

        #region 방어 OR 도주

        #region HPCheck

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
        
        // TODO: check변수를 두개 만들까 아님 애니메이션 평가하는 함수에 job을 넣을까
        INode.NodeState CheckMoreHp()
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

            if (result >= 0.5f) // 정밀한 검사 필요
            {
                return INode.NodeState.Success;
            }
            return INode.NodeState.Failure;
            
            // if (_status.hp.Current / _status.hp.Max > 0.5f)
            // {
            //     return INode.NodeState.Success;
            // }
        }

        #endregion

        #region 방어

        INode.NodeState CheckDeffenceAction()
        {
            if (IsAnimationRunning("돼지 방어"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }
        
        INode.NodeState StartDeffence()
        {
            _animator.SetTrigger("tDeffence");
            return INode.NodeState.Success;
        }

        #endregion

        #region 도주

        INode.NodeState CheckRunAction()
        {
            if (IsAnimationRunning("도주 애니메이션"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        INode.NodeState StartRun()
        {
            _animator.SetTrigger("tRun");
            return INode.NodeState.Success;
        }
        
        #endregion

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
            // TODO : 범위 탐색 코드 구현 필요
            return INode.NodeState.Failure;
        }

        INode.NodeState StartAttack()
        {
            _animator.SetInteger(AttackType, 0);
            return INode.NodeState.Success;
        }
        
        #endregion

        #region Rush

        // 거리 체크 
        INode.NodeState CheckDistance()
        {
            return INode.NodeState.Success;
        }

        #endregion
        

    }
}
