using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorTree.Base;
using Data;
using Fusion;
using Manager;
using State.StateClass;
using State.StateClass.Base;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Allocator = Unity.Collections.Allocator;

namespace BehaviorTree
{
    [RequireComponent(typeof(Animator))]
    public sealed class PiggyBankAI : NetworkBehaviour
    {
        [SerializeField] private float movementSpeed = 1.0f;

        #region Component

        private Rigidbody _rb;
        private BehaviorTreeRunner _btRunner;
        private NetworkMecanimAnimator _animator = null;
        private StatusBase _status;

        private GameManager _gameManager;
        private UserData _userData;
        private GameObject[] _players;

        //[field:SerializeField] // 프로퍼티도 인스펙터에서 보여줌

        #endregion

        #region 속성

        private float _playerCount;
        private float _durationTime;

        [SerializeField] private float attackRange = 3; // 발차기 감지 범위
        [SerializeField] private float rushRange = 10; // 돌진 감지 범위

        private static readonly int Walk = Animator.StringToHash("isWalk");
        
        private static readonly int Dead = Animator.StringToHash("Dead");
        private static readonly int Rest = Animator.StringToHash("Rest");
        private static readonly int Defence = Animator.StringToHash("Defence");
        private static readonly int Attack = Animator.StringToHash("Attack");
        
        private static readonly int AttackType = Animator.StringToHash("Attack_Type");

        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<NetworkMecanimAnimator>();
            _btRunner = new BehaviorTreeRunner(SettingBT());
            _status = GetComponent<MonsterStatus>();
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _playerCount = _gameManager.AlivePlayerCount;

            _players = GameObject.FindGameObjectsWithTag("Player");

            // TODO : foreach문에서 조건문을 계속 호출해서 성능 저하가 일어나는지 테스트 필요
            // foreach (var player in _players)
            // {
            //     _playerPrefabs.Add(player);
            // }
            attackRange = 20;
            rushRange = 100;
            
            _animator.Animator.SetFloat(AttackType, 0);
            _animator.Animator.SetBool(Walk, true);
            
            StartCoroutine(WalkCorutine(0.5f));
            // StartCoroutine(BTCorutine(5.0f));
        }

        private void Update()
        {
            _btRunner.Operator();
        }

        // IEnumerator BTCorutine(float waitTime)
        // {
        //     while (true)
        //     {
        //         _btRunner.Operator();
        //         yield return new WaitForSeconds(waitTime);
        //     }
        // }

        IEnumerator WalkCorutine(float waitTime)
        {
            while (true)
            {
                if (IsAnimationRunning("piggy_walk"))
                {
                    _rb.velocity = new Vector3(0, 0, -movementSpeed);
                }
                else
                {
                    _rb.velocity = new Vector3(0, 0, 0);
                }
                yield return new WaitForSeconds(waitTime);
            }
        }

        INode SettingBT()
        {
            return new SequenceNode
            (
                new List<INode>()
                {
                    // new ActionNode(WalkAround), // Walk
                    // // new ActionNode(StopWalk),
                    // new ActionNode(CheckWalkAction),
                    new SelectorNode
                    (
                        false,
                        // new ActionNode(StopDefence),
                        new SequenceNode
                        (
                            new List<INode>()
                            {   // Deffence
                                new ActionNode(CheckMoreHp), 
                                new ActionNode(StartDefence),
                            }
                        ),
                        new SequenceNode
                        (
                            new List<INode>()
                            {
                                new ActionNode(CheckRunAction), // Run
                                new ActionNode(StartRun),
                            }
                        )
                    ),
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckAttackAction), // Kick
                            new ActionNode(CheckAttackDistance),
                            new ActionNode(StartAttack),
                            new ActionNode(TermFuction),
                        }
                    ),
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new SequenceNode(
                                new List<INode>()
                                {
                                    new ActionNode(CheckRushAction), // Rush
                                    new ActionNode(CheckRushDistance),
                                    new ActionNode(StartRush),
                                    new ActionNode(TermFuction),
                                }
                            ),
                            // new SequenceNode
                            // (
                            //     new List<INode>()
                            //     {
                            //         new ActionNode(CheckJumpAttackAction), // JumpAttack
                            //         new ActionNode(StartJumpAttack),
                            //         // new ActionNode(ReleaseInteger),
                            //     }
                            // )
                        }
                    ),
                    // new SequenceNode
                    // (
                    //     new List<INode>()
                    //     {
                    //         new ActionNode(CheckFartAction), // fart
                    //         new ActionNode(StartFart),
                    //         // new ActionNode(ReleaseInteger),
                    //     }
                    // ),
                    // take a rest
                    // sleep
                }
            );
        }

        bool IsAnimationRunning(string stateName)
        {
            if (_animator != null)
            {
                if (_animator.Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    var normalizedTime = _animator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    // DebugManager.ToDo($"{stateName}진행 시간 : {normalizedTime}");
                    
                    return normalizedTime != 0 && normalizedTime < 1f;
                }
            }

            return false;
        }

        INode.NodeState TermFuction()
        {
            if (_gameManager.PlayTimer - _durationTime > 5.0f)
            {
                return INode.NodeState.Success;
            }
            return INode.NodeState.Running;
        }
        
        #region Walk

        /// <summary>
        /// 돼지저금통이 이동하는 함수 ==> 추후에 정밀하게 이동할 예정
        /// </summary>
        INode.NodeState WalkAround()
        {
            if (IsAnimationRunning("piggy_idle"))
            {
                _animator.Animator.SetBool(Walk, true);
                _rb.velocity = new Vector3(0, 0, -movementSpeed);
                
                StartCoroutine(WalkAnimationWait(3.0f));
            }

            return INode.NodeState.Success;
        }
        
        IEnumerator WalkAnimationWait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _animator.Animator.SetBool(Walk, false);
            _rb.velocity = new Vector3(0, 0, 0);
        }

        INode.NodeState CheckWalkAction()
        {
            if (IsAnimationRunning("piggy_walk"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }
        

        INode.NodeState StopWalk()
        {
            if (IsAnimationRunning("piggy_walk"))
            {
                _animator.Animator.SetBool(Walk, false);
                _rb.velocity = new Vector3(0, 0, 0);
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
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

            // Log.Debug($"CheckHP : {result}");

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

        INode.NodeState StartDefence()
        {
            if (IsAnimationRunning("piggy_defence"))
            {
                return INode.NodeState.Running;
            }
            _animator.Animator.SetTrigger(Defence);
            return INode.NodeState.Success;
        }

        // INode.NodeState StopDefence()
        // {
        //     if (_gameManager.PlayTimer - _durationTime > 3.0f && _animator.GetBool(IsDefence))
        //     {
        //         _animator.SetBool(IsDefence, false);
        //         _durationTime = 0;
        //         return INode.NodeState.Success;
        //     }
        //
        //     return INode.NodeState.Failure;
        // }

        #endregion

        #region 도주

        INode.NodeState CheckRunAction()
        {
            // TODO : 도주는 어떻게 해야될까? Walk로 해야하나?
            // if (IsAnimationRunning("도주 애니메이션"))
            // {
            //     return INode.NodeState.Running;
            // }
            return INode.NodeState.Success;
        }

        INode.NodeState StartRun()
        {
            // _animator.SetTrigger("tRun");
            return INode.NodeState.Success;
        }
        
        #endregion

        #endregion
        
        #region Attack

        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB) {
            return math.distance(pointA, pointB);
        }
        
        private struct CheckDistanceJob : IJobParallelFor
        {
            public NativeArray<bool> Result;
            public NativeArray<Vector3> PlayerPosition;
            public Vector3 PiggyPosition;
            public float DetectingRange;
            
            public void Execute(int index)
            {
                if (FastDistance(PiggyPosition, PlayerPosition[index]) < DetectingRange)
                    Result[index] = true;
                else
                    Result[index] = false;
            }
        }
        
        /// <summary>
        /// 돼지저금통이 공격을 하는중인지 판단하는 함수
        /// </summary>
        INode.NodeState CheckAttackAction()
        {
            if (IsAnimationRunning("Attack_Blend"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        INode.NodeState CheckAttackDistance()
        {
            // TODO : 범위 탐색 코드 구현 필요
            // TODO : NativeArraty를 계속 사용하면 성능 저하 가능성 있으니, 일반 멤버변수로 만드는 방법으로 벤치마킹 해보자.

            // NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            // NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);
            
            NativeArray<bool> results = new NativeArray<bool>(1, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>(1, Allocator.TempJob);

            Vector3 piggyPosition = transform.position;

            // for (int index = 0; index < (int)_playerCount; index++)
            // {
            //     playerPosition[index] = _players[index].transform.position;
            // }
            
            for (int index = 0; index < 1; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }
            
            CheckDistanceJob job = new CheckDistanceJob()
            {
                Result = results,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingRange = attackRange
            };
            
            // TODO : 배치크기는 어떻게해야 가장 효율이 좋을까?
            // JobHandle jobHandle = job.Schedule((int)_playerCount, 3);
            JobHandle jobHandle = job.Schedule(1, 3);
            jobHandle.Complete();

            bool checkResult = false;
            
            foreach (var result in results)
            {
                checkResult |= result;
            }

            results.Dispose();
            playerPosition.Dispose();
            
            if (checkResult)
            {
                return INode.NodeState.Success;
            }
            
            return INode.NodeState.Failure;
        }

        INode.NodeState StartAttack()
        {
            _animator.Animator.SetFloat(AttackType, 0.0f);
            // 바로 넘어가면서 값이 변한다. ==> 사이에 간격을 띄우자
            _animator.Animator.SetTrigger(Attack);
            _durationTime = _gameManager.PlayTimer;
            
            return INode.NodeState.Success;
        }

        // INode.NodeState ReleaseInteger()
        // {
        //     _animator.SetFloat(AttackType, -1.0f);
        //     return INode.NodeState.Success;
        // }
        
        #endregion

        #region Rush
        
        INode.NodeState CheckRushAction()
        {
            if (IsAnimationRunning("Attack_Blend"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }
        
        // 거리 체크 
        INode.NodeState CheckRushDistance()
        {
            // NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            // NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);
            
            NativeArray<bool> results = new NativeArray<bool>(1, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>(1, Allocator.TempJob);
            
            Vector3 piggyPosition = transform.position;

            for (int index = 0; index < (int)_playerCount; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }
            
            CheckDistanceJob job = new CheckDistanceJob()
            {
                Result = results,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingRange = rushRange
            };
            
            // TODO : 배치크기는 어떻게해야 가장 효율이 좋을까?
            // JobHandle jobHandle = job.Schedule((int)_playerCount, 3);            
            JobHandle jobHandle = job.Schedule(1, 3);

            
            jobHandle.Complete();

            bool checkResult = false;
            
            foreach (var result in results)
            {
                checkResult |= result;
            }

            results.Dispose();
            playerPosition.Dispose();

            if (checkResult)
            {
                return INode.NodeState.Success;
            }
            
            return INode.NodeState.Failure;
        }
        
        INode.NodeState StartRush()
        {
            _animator.Animator.SetFloat(AttackType, 3);
            _animator.Animator.SetTrigger(Attack);
            _durationTime = _gameManager.PlayTimer;
            
            return INode.NodeState.Success;
        }

        #endregion

        #region JumpAttack

        INode.NodeState CheckJumpAttackAction()
        {
            if (IsAnimationRunning("Attack_Blend"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }
        
        INode.NodeState StartJumpAttack()
        {
            _animator.Animator.SetInteger(AttackType, 2);
            return INode.NodeState.Success;
        }

        #endregion

        #region Fart

        INode.NodeState CheckFartAction()
        {
            if (IsAnimationRunning("Attack_Blend"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }
        
        INode.NodeState StartFart()
        {
            _animator.Animator.SetFloat(AttackType, 4);
            return INode.NodeState.Success;
        }

        #endregion
        
    }
}
