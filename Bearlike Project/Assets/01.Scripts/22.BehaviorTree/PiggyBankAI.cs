using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay;
using State.StateClass;
using State.StateClass.Base;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
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
        private VisualEffect _visualEffect;
        private NavMeshAgent _navMeshAgent;

        private GameManager _gameManager;
        private UserData _userData;
        private GameObject[] _players;

        //[field:SerializeField] // 프로퍼티도 인스펙터에서 보여줌

        #endregion

        #region 속성

        private float _playerCount;
        private float _durationTime; // Action간의 딜레이 시간
        private int _targetPlayerIndex; // target으로 지정되는 Player의 index

        #region 회전

        private const float RotationDuration = 1.0f;

        private Quaternion _targetRotation; // 목표 회전값
        private float _timePassed = 0f; // 회전 보간에 사용될 시간 변수

        private float _rotationlastTime;

        #endregion

        private bool isDead = false;

        [SerializeField] private float attackRange = 10; // 발차기 감지 범위
        [SerializeField] private float rushRange = 100; // 돌진 감지 범위
        [SerializeField] private float coinAtaackMinRange = 10; // 코인 공격 최소 감지 범위
        [SerializeField] private float coinAtaackMaxRange = 100; // 코인 공격 최대 감지 범위

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
            _visualEffect = GetComponentInChildren<VisualEffect>();
            if(TryGetComponent(out _navMeshAgent)== false) _navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _playerCount = _gameManager.AlivePlayerCount;

            attackRange = 10;
            rushRange = 100;
            coinAtaackMinRange = 50;
            coinAtaackMaxRange = 150;

            isDead = false;

            _animator.Animator.SetFloat(AttackType, 0);
            _animator.Animator.SetBool(Walk, true);

            StartCoroutine(WalkCoroutine(0.5f));
            StartCoroutine(DieCoroutine(0.5f));
        }

        public override void Spawned()
        {
            base.Spawned();
            
            List<GameObject> playerObjects = new List<GameObject>();
            foreach (var playerRef in Runner.ActivePlayers.ToArray())
            {
                playerObjects.Add(Runner.GetPlayerObject(playerRef).gameObject);
            }
            _players = playerObjects.ToArray();
        }

        public override void FixedUpdateNetwork()
        {
            if (!isDead)
                _btRunner.Operator();
        }

        IEnumerator DieCoroutine(float waitTime)
        {
            while (true)
            {
                if (_status.IsDie)
                {
                    _animator.Animator.SetTrigger(Dead);
                    isDead = true;
                    yield break;
                }

                yield return new WaitForSeconds(waitTime);
            }
        }

        IEnumerator WalkCoroutine(float waitTime)
        {
            while (true)
            {
                if (IsAnimationRunning("piggy_walk"))
                {
                    if (FastDistance(transform.position, _players[0].transform.position) > 10.0f)
                    {
                        _navMeshAgent.speed = 2.0f;
                        _navMeshAgent.SetDestination(_players[0].transform.position);
                    }
                    else
                    {
                        _navMeshAgent.speed = 0.0f;
                    }
                }
                
                yield return new WaitForSeconds(waitTime);
            }
        }

        // INode SettingBT()
        // {
        //     return new SequenceNode
        //     (
        //         new List<INode>()
        //         {
        //             new SelectorNode
        //             (
        //                 new List<INode>()
        //                 {
        //                     new SequenceNode
        //                     (
        //                         new List<INode>()
        //                         {   // Deffence
        //                             new ActionNode(CheckMoreHp), 
        //                             new ActionNode(StartDefence),
        //                             new ActionNode(TermFuction),
        //                         }
        //                     ),
        //                     new SequenceNode
        //                     (
        //                         new List<INode>()
        //                         {   // Run
        //                             new ActionNode(StartRun),
        //                             new ActionNode(TermFuction),
        //                             new ActionNode(StopRun),
        //                         }
        //                     )
        //                 }
        //             ),
        //             new SelectorNode(
        //                 new List<INode>(){
        //                     new SequenceNode
        //                     (
        //                         new List<INode>()
        //                         {
        //                             new ActionNode(CheckAttackAction), // Kick
        //                             new ActionNode(CheckAttackDistance),
        //                             new ActionNode(StartRotate),
        //                             new ActionNode(StartAttack),
        //                             new ActionNode(TermFuction),
        //                         }
        //                     ),
        //                     new ActionNode(SuccessFunction),
        //                 }
        //             ),
        //             new SelectorNode
        //             (
        //                 new List<INode>()
        //                 {
        //                     new SequenceNode(
        //                         new List<INode>()
        //                         {
        //                             new ActionNode(CheckRushAction), // Rush
        //                             new ActionNode(CheckRushDistance),
        //                             new ActionNode(StartRotate),
        //                             new ActionNode(StartRush),
        //                             new ActionNode(TermFuction),
        //                         }
        //                     ),
        //                     new SequenceNode
        //                     (
        //                         new List<INode>()
        //                         {
        //                             new ActionNode(CheckJumpAttackAction), // JumpAttack
        //                             new ActionNode(StartRotate),
        //                             new ActionNode(StartJumpAttack),
        //                             new ActionNode(TermFuction),
        //                         }
        //                     )
        //                 }
        //             ),
        //             new SequenceNode
        //             (
        //                 new List<INode>()
        //                 {
        //                     new ActionNode(CheckFartAction), // fart
        //                     new ActionNode(StartFart),
        //                     new ActionNode(TermFuction),
        //                 }
        //             ),
        //             new SequenceNode
        //             (    // take a rest
        //                 new List<INode>()
        //                 {
        //                     new ActionNode(CheckRestAction),
        //                     new ActionNode(CheckRestHp),
        //                     new ActionNode(StartRest),
        //                     new ActionNode(TermFuction),
        //                 }
        //             ),
        //             new SequenceNode
        //             (   // CoinAttack
        //                 new List<INode>()
        //                 {
        //                     new ActionNode(CheckCoinAttackAction),
        //                     new ActionNode(CheckCoinAttackDistance),
        //                     new ActionNode(StartCoinAttack),
        //                     new ActionNode(TermFuction),
        //                 }
        //             ),
        //             
        //             // // sleep
        //         }
        //     );
        // }

        INode SettingBT()
        {
            return new SequenceNode
            (
                new SequenceNode
                (
                    new ActionNode(CheckMoreHp),
                    new ActionNode(StartDefence),
                    new ActionNode(TermFuction)
                )//,
                // new SequenceNode
                // (
                //     new ActionNode(StartRun),
                //     new ActionNode(TermFuction),
                //     new ActionNode(StopRun)
                // ),
                // new SequenceNode
                // (
                //     new ActionNode(CheckAttackAction), // Kick
                //     new ActionNode(CheckAttackDistance),
                //     new ActionNode(StartRotate),
                //     new ActionNode(StartAttack),
                //     new ActionNode(TermFuction)
                // ),
                // new SequenceNode
                // (
                //     new ActionNode(CheckRushAction), // Rush
                //     new ActionNode(CheckRushDistance),
                //     new ActionNode(StartRotate),
                //     new ActionNode(StartRush),
                //     new ActionNode(TermFuction)
                // ),
                // new SequenceNode
                // (
                //     new ActionNode(CheckJumpAttackAction), // JumpAttack
                //     new ActionNode(StartRotate),
                //     new ActionNode(StartJumpAttack),
                //     new ActionNode(TermFuction)
                // ),
                // new SequenceNode
                // (
                //     new ActionNode(CheckFartAction), // fart
                //     new ActionNode(StartFart),
                //     new ActionNode(TermFuction)
                // ),
                // new SequenceNode
                // ( // take a rest
                //     new ActionNode(CheckRestAction),
                //     new ActionNode(CheckRestHp),
                //     new ActionNode(StartRest),
                //     new ActionNode(TermFuction)
                // ),
                // new SequenceNode
                // ( // CoinAttack
                //     new ActionNode(CheckCoinAttackAction),
                //     new ActionNode(CheckCoinAttackDistance),
                //     new ActionNode(StartCoinAttack),
                //     new ActionNode(TermFuction)
                // )
                // // // sleep
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

        #region VFX Function

        void PlayVFX(string vfxName)
        {
            GameObject targetObject = transform.Find(vfxName).gameObject;

            if (targetObject != null)
            {
                _visualEffect = targetObject.GetComponent<VisualEffect>();
                if (_visualEffect != null)
                {
                    _visualEffect.Play();
                }
            }
        }

        void StopVFX(string vfxName)
        {
            GameObject targetObject = transform.Find(vfxName).gameObject;

            if (targetObject != null)
            {
                _visualEffect = targetObject.GetComponent<VisualEffect>();
                if (_visualEffect != null)
                {
                    _visualEffect.Stop();
                }
            }
        }

        #endregion

        INode.NodeState TermFuction()
        {
            if (_gameManager.PlayTimer - _durationTime > 5.0f)
            {
                _visualEffect.Stop();
                _animator.Animator.Play("piggy_walk");

                return INode.NodeState.Success;
            }

            return INode.NodeState.Running;
        }

        INode.NodeState SuccessFunction()
        {
            return INode.NodeState.Success;
        }

        #region Walk

        /// <summary>
        /// 돼지저금통이 이동하는 함수 ==> 추후에 정밀하게 이동할 예정
        /// </summary>
        // INode.NodeState WalkAround()
        // {
        //     if (IsAnimationRunning("piggy_idle"))
        //     {
        //         _animator.Animator.SetBool(Walk, true);
        //         _rb.velocity = new Vector3(0, 0, -movementSpeed);
        //         
        //         StartCoroutine(WalkAnimationWait(3.0f));
        //     }
        //
        //     return INode.NodeState.Success;
        // }
        //
        // IEnumerator WalkAnimationWait(float waitTime)
        // {
        //     yield return new WaitForSeconds(waitTime);
        //     _animator.Animator.SetBool(Walk, false);
        //     _rb.velocity = new Vector3(0, 0, 0);
        // }
        //
        // INode.NodeState CheckWalkAction()
        // {
        //     if (IsAnimationRunning("piggy_walk"))
        //     {
        //         return INode.NodeState.Running;
        //     }
        //
        //     return INode.NodeState.Success;
        // }
        //
        //
        // INode.NodeState StopWalk()
        // {
        //     if (IsAnimationRunning("piggy_walk"))
        //     {
        //         _animator.Animator.SetBool(Walk, false);
        //         _rb.velocity = new Vector3(0, 0, 0);
        //         return INode.NodeState.Success;
        //     }
        //
        //     return INode.NodeState.Failure;
        // }
        INode.NodeState StartWalk()
        {
            //TODO : 플레이어의 거리를 측정하거나 랜덤한 좌표에 가는 코드를 구현해야한다
            // 그러려면 target을 가까운 플레이어나 유리한 위치를 찾는 알고리즘이 필요하다.
            // GameObject target = 
            // _agent.SetDestination(target.position);

            return INode.NodeState.Success;
        }

        #endregion

        #region 방어 OR 도주

        #region HPCheck

        INode.NodeState CheckMoreHp()
        {
            if ((_status.hp.Current / _status.hp.Max) >= 0.5f) // 정밀한 검사 필요
            {
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        #endregion

        #region 방어

        INode.NodeState StartDefence()
        {
            if (IsAnimationRunning("piggy_defence"))
            {
                return INode.NodeState.Running;
            }

            _visualEffect.Play();
            _animator.Animator.SetTrigger(Defence);
            _navMeshAgent.speed = 0.0f;
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        #endregion

        #region 도주

        INode.NodeState StartRun()
        {
            if (IsAnimationRunning("piggy_run"))
            {
                return INode.NodeState.Running;
            }

            _animator.SetTrigger("Run");
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        INode.NodeState StopRun()
        {
            _animator.Animator.Play("piggy_idle");

            return INode.NodeState.Success;
        }

        #endregion

        #endregion

        #region Attack

        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }

        private struct CheckDistanceJob : IJobParallelFor
        {
            public NativeArray<bool> Results;
            public NativeArray<float> Distances;
            public NativeArray<Vector3> PlayerPosition;
            public Vector3 PiggyPosition;
            public float DetectingRange;

            public void Execute(int index)
            {
                var distance = FastDistance(PiggyPosition, PlayerPosition[index]);
                if (distance < DetectingRange)
                {
                    Results[index] = true;
                }
                else
                {
                    Results[index] = false;
                }

                Distances[index] = distance;
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

            NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            NativeArray<float> distances = new NativeArray<float>((int)_playerCount, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);

            Vector3 piggyPosition = transform.position;

            for (int index = 0; index < 1; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }

            CheckDistanceJob job = new CheckDistanceJob()
            {
                Results = results,
                Distances = distances,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingRange = attackRange
            };

            // TODO : 배치크기는 어떻게해야 가장 효율이 좋을까?
            JobHandle jobHandle = job.Schedule((int)_playerCount, 3);
            jobHandle.Complete();

            bool checkResult = false;
            float maxDistance = attackRange;

            for (int index = 0; index < (int)_playerCount; ++index)
            {
                if (results[index] && (distances[index] < maxDistance))
                {
                    checkResult |= results[index];
                    maxDistance = distances[index];
                    _targetPlayerIndex = index;
                }
            }

            results.Dispose();
            distances.Dispose();
            playerPosition.Dispose();

            if (checkResult)
            {
                Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
                _targetRotation = Quaternion.LookRotation(targetDirection);

                _rotationlastTime = _gameManager.PlayTimer;

                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRotate()
        {
            _timePassed += _gameManager.PlayTimer - _rotationlastTime;

            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _timePassed / RotationDuration);

            if (_timePassed >= RotationDuration)
            {
                _timePassed = 0.0f;
                return INode.NodeState.Success;
            }

            _rotationlastTime = _gameManager.PlayTimer;

            return INode.NodeState.Running;
        }

        INode.NodeState StartAttack()
        {
            _animator.Animator.SetFloat(AttackType, 0.0f);
            _animator.Animator.SetTrigger(Attack);
            _navMeshAgent.speed = 0.0f;
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

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

        // TODO : 러쉬의 범위를 제안하면 점프 공격의 패턴이 거의 안나올 가능성이 있기에 러쉬의 범위제한을 없애는 방향으로 가거나 점프공격을 포물선으로 움직이게 하면 되지 않을까
        // 거리 체크 
        INode.NodeState CheckRushDistance()
        {
            NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            NativeArray<float> distances = new NativeArray<float>((int)_playerCount, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);

            Vector3 piggyPosition = transform.position;

            for (int index = 0; index < (int)_playerCount; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }

            CheckDistanceJob job = new CheckDistanceJob()
            {
                Results = results,
                Distances = distances,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingRange = rushRange
            };

            // TODO : 배치크기는 어떻게해야 가장 효율이 좋을까?
            JobHandle jobHandle = job.Schedule((int)_playerCount, 3);

            jobHandle.Complete();

            bool checkResult = false;
            float maxDistance = rushRange;

            for (int index = 0; index < (int)_playerCount; ++index)
            {
                if (results[index] && (distances[index] < maxDistance))
                {
                    checkResult |= results[index];
                    maxDistance = distances[index];
                    _targetPlayerIndex = index;
                }
            }

            results.Dispose();
            distances.Dispose();
            playerPosition.Dispose();

            if (checkResult)
            {
                Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
                _targetRotation = Quaternion.LookRotation(targetDirection);

                _rotationlastTime = _gameManager.PlayTimer;

                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRush()
        {
            _animator.Animator.SetFloat(AttackType, 3);
            _animator.Animator.SetTrigger(Attack);

            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position - _players[_targetPlayerIndex].transform.forward * 20);
            _navMeshAgent.speed = 10.0f;

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

        INode.NodeState CheckJumpAttackDistance()
        {
            NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            NativeArray<float> distances = new NativeArray<float>((int)_playerCount, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);

            Vector3 piggyPosition = transform.position;

            for (int index = 0; index < (int)_playerCount; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }

            CheckDistanceJob job = new CheckDistanceJob()
            {
                Results = results,
                Distances = distances,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingRange = rushRange
            };

            // TODO : 배치크기는 어떻게해야 가장 효율이 좋을까?
            JobHandle jobHandle = job.Schedule((int)_playerCount, 3);

            jobHandle.Complete();

            float maxDistance = 999.0f;

            for (int index = 0; index < (int)_playerCount; ++index)
            {
                if (distances[index] < maxDistance)
                {
                    maxDistance = distances[index];
                    _targetPlayerIndex = index;
                }
            }

            results.Dispose();
            distances.Dispose();
            playerPosition.Dispose();

            Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
            _targetRotation = Quaternion.LookRotation(targetDirection);

            _rotationlastTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        INode.NodeState StartJumpAttack()
        {
            _animator.Animator.SetInteger(AttackType, 2);

            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position);
            _navMeshAgent.speed = 10.0f;

            // jump하는 함수 구현

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
            _navMeshAgent.speed = 0.0f;

            return INode.NodeState.Success;
        }

        #endregion

        #region Rest

        INode.NodeState CheckRestAction()
        {
            if (IsAnimationRunning("piggy_rest"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        INode.NodeState CheckRestHp()
        {
            if ((_status.hp.Current / _status.hp.Max) <= 0.5f) // 정밀한 검사 필요
            {
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRest()
        {
            _animator.Animator.SetTrigger(Rest);
            _navMeshAgent.speed = 0.0f;

            return INode.NodeState.Success;
        }

        #endregion

        #region CoinAttack

        INode.NodeState CheckCoinAttackAction()
        {
            if (IsAnimationRunning("Attack_Blend"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        private struct CheckCoinAttackDistanceJob : IJobParallelFor
        {
            public NativeArray<bool> Results;
            public NativeArray<Vector3> PlayerPosition;
            public Vector3 PiggyPosition;
            public float DetectingMaxRange;
            public float DetectingMinRange;

            public void Execute(int index)
            {
                var distance = FastDistance(PiggyPosition, PlayerPosition[index]);
                if (DetectingMinRange <= distance && distance <= DetectingMaxRange)
                {
                    Results[index] = true;
                }
                else
                {
                    Results[index] = false;
                }
            }
        }

        INode.NodeState CheckCoinAttackDistance()
        {
            NativeArray<bool> results = new NativeArray<bool>((int)_playerCount, Allocator.TempJob);
            NativeArray<Vector3> playerPosition = new NativeArray<Vector3>((int)_playerCount, Allocator.TempJob);

            Vector3 piggyPosition = transform.position;

            for (int index = 0; index < (int)_playerCount; index++)
            {
                playerPosition[index] = _players[index].transform.position;
            }

            CheckCoinAttackDistanceJob job = new CheckCoinAttackDistanceJob()
            {
                Results = results,
                PlayerPosition = playerPosition,
                PiggyPosition = piggyPosition,
                DetectingMaxRange = coinAtaackMaxRange,
                DetectingMinRange = coinAtaackMinRange,
            };

            JobHandle jobHandle = job.Schedule((int)_playerCount, 3);

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

        INode.NodeState StartCoinAttack()
        {
            _animator.Animator.SetTrigger(Attack);
            _animator.Animator.SetFloat(AttackType, 1);

            // TODO : VFX를 받아와서 실행하고, 매개변수로 자식 객체의 이름를 받는 함수구현


            return INode.NodeState.Success;
        }

        #endregion
    }
}