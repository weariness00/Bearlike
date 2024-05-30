using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay;
using Manager;
using Player;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Monster.Container
{
    public class PiggyBank : MonsterBase
    {
        #region Component
        
        private BehaviorTreeRunner _btRunner;
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

        private const float RotationDuration = 0.5f;

        private Vector3 _lookDirection;
        private Vector3 _targetRotation; // 목표 회전값
        private float _timePassed = 0f; // 회전 보간에 사용될 시간 변수

        #endregion

        private bool isDead = false;

        [Header("공격 범위")]
        [SerializeField] private float attackRange = 10; // 발차기 감지 범위
        [SerializeField] private float rushRange = 50; // 돌진 감지 범위
        [SerializeField] private float jumpRange = 30; // 점프 감지 범위
        [SerializeField] private float coinAtaackMinRange = 15; // 코인 공격 최소 감지 범위
        [SerializeField] private float coinAtaackMaxRange = 30; // 코인 공격 최대 감지 범위

        [SerializeField] private float jumpAttackDamageRange = 17;
        [SerializeField] private float fartDamageRange = 10;
        //
        [Header("상하 속도")] 
        [SerializeField] private float upTime = 3.0f;
        [SerializeField] private float downTime = 1.0f;
        
        [Header("이동 정도")] 
        [SerializeField]private float runDistance = 10.0f;
        [SerializeField] private float rushDistance = 10.0f;
        [SerializeField]private float jumpHeight = 10.0f;

        private float _delayTime = 5.0f;

        private static readonly int Walk = Animator.StringToHash("tWalk");
        private static readonly int Dead = Animator.StringToHash("tDead");
        private static readonly int Rest = Animator.StringToHash("tRest");
        private static readonly int EndRest = Animator.StringToHash("tEndRest");
        private static readonly int Defence = Animator.StringToHash("tDefence");
        private static readonly int Attack = Animator.StringToHash("tAttack");

        private static readonly int AttackBlend = Animator.StringToHash("AttackBlend");

        private const int ATTACK_TYPE = 0;
        private const int FART_TYPE = 1;
        private const int COIN_TYPE = 2;
        private const int JUMP_TYPE = 3;
        private const int RUSH_TYPE = 4;//

        #endregion

        private void Awake()
        {
            base.Awake();
            _btRunner = new BehaviorTreeRunner(SettingBT());
            _visualEffect = GetComponentInChildren<VisualEffect>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.enabled = false;
            DieAction += () =>
            {
                networkAnimator.Animator.SetTrigger(Dead);
                isDead = true;
            };
        }

        public override void Start()
        {
            base.Start();
            _gameManager = GameManager.Instance; 
            _playerCount = _gameManager.AlivePlayerCount;
            
            isDead = false;

            networkAnimator.Animator.SetFloat(AttackBlend, 0);
        }

        public override void Spawned()
        {
            base.Spawned();
            _navMeshAgent.enabled = true;   
            
            List<GameObject> playerObjects = new List<GameObject>();
            foreach (var playerRef in Runner.ActivePlayers.ToArray())
            {
                playerObjects.Add(Runner.GetPlayerObject(playerRef).gameObject);
            }
            _players = playerObjects.ToArray();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if (!isDead)
                _btRunner.Operator();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.TryGetComponent(out StatusBase otherStatus))
            {
                // TODO : 충돌 데미지 DB에서 밸런스 맞추자
                if(other.gameObject.layer != LayerMask.NameToLayer("Bullet"))
                    otherStatus.PlayerApplyDamage(1, gameObject.GetComponent<NetworkObject>().Id);
            }
        }
        
        #region BT

        INode SettingBT()
        {
            return new SequenceNode // selector로 변경 가능
            (
                // true,
                new SequenceNode
                (
                    new ActionNode(CheckWalkAction),
                    new ActionNode(WalkAround),
                    new ActionNode(TermFuction),
                    new ActionNode(WalkStop)
                ),
                new SelectorNode
                (
                    true,
                    new SequenceNode
                    ( // Deffence
                        new ActionNode(CheckMoreHp),
                        new ActionNode(StartDefence),
                        new ActionNode(TermFuction),
                        new ActionNode(StopDefence)
                    ),
                    new SequenceNode
                    ( 
                        new SequenceNode
                        (   // Run
                            new ActionNode(StartRun),
                            new ActionNode(TermFuction),
                            new ActionNode(StopRun)
                        ),
                        new SequenceNode
                        (   // JumpAttack OR Jump CoinAttack
                            new ActionNode(CheckJumpAttackAction),
                            new ActionNode(CheckJumpAttackDistance),
                            new ActionNode(StartJumpAction),
                            new ActionNode(TermFuction),
                            new ActionNode(StopJumpAttack)
                        )
                    )
                ),
                new SelectorNode(
                    false,
                    new SequenceNode
                    (   // Kick
                        new ActionNode(CheckAttackAction),
                        new ActionNode(CheckAttackDistance),
                        new ActionNode(StartAttack),
                        new ActionNode(TermFuction)
                    ),
                new ActionNode(SuccessFunction)
                ),
                new SelectorNode
                (
                    true,
                    new SelectorNode(
                        true,
                        new SequenceNode
                        (   // Rush
                            new ActionNode(CheckRushAction), 
                            new ActionNode(CheckRushDistance),
                            new ActionNode(StartRush),
                            new ActionNode(TermFuction)
                        ),
                        new SelectorNode(
                            false,
                            new SequenceNode
                            (    // JumpAttack OR Fake JumpAttack
                                new ActionNode(CheckJumpAttackAction), 
                                new ActionNode(StartJumpAttackAction),
                                new ActionNode(TermFuction),
                                new ActionNode(StopJumpAttack)
                            ),
                            new ActionNode(SuccessFunction)
                        )
                    ),
                    new SequenceNode
                    (   // fart
                        new ActionNode(CheckFartAction), 
                        new ActionNode(StartFart),
                        new ActionNode(TermFuction),
                        new ActionNode(StopFart)
                    )
                ),
                new SequenceNode
                (   // Ground CoinAttack
                    new ActionNode(CheckCoinAttackAction),
                    // new ActionNode(CheckCoinAttackDistance),
                    new ActionNode(StartCoinAttack),
                    new ActionNode(TermFuction),
                    new ActionNode(StopCoinAttack)
                ),
                new SequenceNode
                (   // Rest
                    new ActionNode(CheckRestAction),
                    // new ActionNode(CheckRestHp),
                    new ActionNode(StartRest),
                    new ActionNode(TermFuction)
                )
            );
        }

        INode SettingTestBT()
        {
            return new SequenceNode
            (
                // new SequenceNode
                // (
                //     new ActionNode(CheckWalkAction),
                //     new ActionNode(WalkAround),
                //     new ActionNode(TermFuction),
                //     new ActionNode(WalkStop)
                // ),
                // new SelectorNode
                // (
                //     true,
                //     new SequenceNode
                //     ( // Deffence
                //         new ActionNode(CheckMoreHp),
                //         new ActionNode(StartDefence),
                //         new ActionNode(TermFuction),
                //         new ActionNode(StopDefence)
                //     ),
                //     new SequenceNode
                //     ( 
                //         new SequenceNode
                //         (   // Run
                // new ActionNode(StartRun),
                // new ActionNode(TermFuction),
                // new ActionNode(StopRun)
                //         ),
                // new SequenceNode
                // (   // JumpAttack OR Jump CoinAttack
                // new ActionNode(CheckJumpAttackAction),
                // // new ActionNode(CheckJumpAttackDistance),
                // new ActionNode(StartJumpAction),
                // new ActionNode(TermFuction),
                // new ActionNode(StopJumpAttack)
                // ),
                //     )
                // ),
                // new SelectorNode(
                //     false,
                // new SequenceNode
                // (   // Kick
                //     new ActionNode(CheckAttackAction),
                //     // new ActionNode(CheckAttackDistance),
                //     new ActionNode(StartAttack),
                //     new ActionNode(TermFuction)
                // )
                //     new ActionNode(SuccessFunction)
                // ),
                // new SelectorNode
                // (
                //     true,
                //     new SelectorNode(
                //         true,
                //         new SequenceNode
                //         (   // Rush
                new ActionNode(CheckRushAction), 
                new ActionNode(CheckRushDistance),
                new ActionNode(StartRush),
                new ActionNode(TermFuction)
                //         ),
                //         new SelectorNode(
                //             false,
                // new SequenceNode
                // (    // JumpAttack OR Fake JumpAttack
                //     new ActionNode(CheckJumpAttackAction), 
                //     new ActionNode(StartJumpAttackAction),
                //     new ActionNode(TermFuction),
                //     new ActionNode(StopJumpAttack)
                // ),
                //             new ActionNode(SuccessFunction)
                //         )
                //     ),
                // new SequenceNode
                // (   // fart
                //     new ActionNode(CheckFartAction), 
                //     new ActionNode(StartFart),
                //     new ActionNode(TermFuction),
                //     new ActionNode(StopFart)
                // ),
                // ),
                // new SequenceNode
                // (   // Ground CoinAttack
                //     new ActionNode(CheckCoinAttackAction),
                //     // new ActionNode(CheckCoinAttackDistance),
                //     new ActionNode(StartCoinAttack),
                //     new ActionNode(TermFuction),
                //     new ActionNode(StopCoinAttack)
                // )
                // new SequenceNode
                // (   // Rest
                //     new ActionNode(CheckRestAction),
                //     // new ActionNode(CheckRestHp),
                //     new ActionNode(StartRest),
                //     new ActionNode(TermFuction)
                // )
            );
        }

        /// <summary>
        /// 파라미터로 넘어오는 애니메이션이 동작 중인지 확인하는 함수
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        bool IsAnimationRunning(string stateName)
        {
            if (networkAnimator != null)
            {
                if (networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    var normalizedTime = networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    // DebugManager.ToDo($"{stateName}진행 시간 : {normalizedTime}");

                    return normalizedTime != 0 && normalizedTime < 1f;
                }
            }
            return false;
        }

        #region AdditionalNode

        /// <summary>
        /// delayTime동안 지연시간을 부여하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState TermFuction()
        {
            if (_gameManager.PlayTimer - _durationTime > _delayTime)
                return INode.NodeState.Success;

            return INode.NodeState.Running;
        }

        /// <summary>
        /// 무조건 성공을 반환하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState SuccessFunction()
        {
            return INode.NodeState.Success;
        }

        #endregion

        #region Calculation Furntion
        /// <summary>
        /// 거리계산을 해주는 함수
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns></returns>
        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }

        #endregion
        
        #region Walk
        
        /// <summary>
        /// 걷기 행동이 실행중인지 판단하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckWalkAction()
        {
            if (IsAnimationRunning("Piggy_Walk"))
            {
                return INode.NodeState.Running;
            }
        
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 걷기 행동을 실행하는 노드
        /// </summary>
        INode.NodeState WalkAround()
        {
            networkAnimator.Animator.SetTrigger(Walk);
                
            // TODO: 위치를 어디 잡을까 ==> 
            // 가장 피가 적은 플레이어에게 이동?
            int minHp = -1;
            Vector3 direction = new Vector3();
                
            foreach (var player in _players)
            {
                var hp = player.GetComponent<PlayerStatus>().hp;
                if (hp < minHp)
                {
                    minHp = hp;
                    direction = player.transform.position - transform.position;
                }
            }
            direction = direction.normalized * (direction.magnitude - (attackRange - 5.0f));
            // TODO : 속도를 잘 설정해야한다.
            _navMeshAgent.speed = direction.magnitude / 5.0f;
            _navMeshAgent.SetDestination(transform.position + direction);
                
            _durationTime = _gameManager.PlayTimer;
                
            return INode.NodeState.Success;
        }

        INode.NodeState WalkStop()
        {
            _navMeshAgent.SetDestination(transform.position);
            // networkAnimator.Animator.Play("Piggy_Idle");

            return INode.NodeState.Success;
        }
        
        #endregion

        #region 방어 OR 도주

        #region HPCheck

        /// <summary>
        /// HP가 50%이상인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckMoreHp()
        {
            if (status.hp.Current / (float)status.hp.Max > 0.5f)
            {
                return INode.NodeState.Success;
            }
            return INode.NodeState.Failure;
        }

        #endregion

        #region 방어

        /// <summary>
        /// Defence 동작을 실행하는 노드 (애니메이션, VFX)
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartDefence()
        {
            if (IsAnimationRunning("Piggy_Defence"))
            {
                return INode.NodeState.Running;
            }
            
            PlayVFXRPC("Shield_vfx");
            networkAnimator.Animator.SetTrigger(Defence);
            _navMeshAgent.speed = 0.0f;

            ApplyDefenceRPC();
            
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        /// <summary>
        /// Defence 동작을 그만하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StopDefence()
        {
            StopVFXRPC("Shield_vfx");

            RemoveDefenceRPC();
            
            return INode.NodeState.Success;
        }
        
        #endregion

        #region 도주

        /// <summary>
        /// 도주 행동을 실행하는 노드
        /// 플레이어들의 위치들의 무게중심에서 멀어지는 방향을 설정하여 도주한다.
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartRun()
        {
            if (IsAnimationRunning("Piggy_Run"))
            {
                return INode.NodeState.Running;
            }

            Vector3 centroid = new Vector3();
            foreach(var player in _players)
            {
                var position = player.transform.position;
                centroid.x += position.x;
                centroid.y += position.y;
                centroid.z += position.z;
            }
            centroid.x /= _playerCount;
            centroid.y /= _playerCount;
            centroid.z /= _playerCount;

            var direction = (transform.position - centroid).normalized;
            
            networkAnimator.Animator.SetFloat(AttackBlend, RUSH_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);
            
            _navMeshAgent.speed = runDistance / 3.0f;   // animation의 길이가 3초
            _navMeshAgent.SetDestination(transform.position + direction * runDistance);
            
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        /// <summary>
        /// 도주 행동을 정지하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StopRun()
        {
            // TODO : vfx 끄기

            return INode.NodeState.Success;
        }

        #endregion

        #endregion

        #region Attack

        /// <summary>
        /// 공격 행동을 하는중인지 판단하는 노드
        /// </summary>
        INode.NodeState CheckAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        /// <summary>
        /// 주변에 공격을 할수있는 플레이어가 있는지 판단하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckAttackDistance()
        {
            bool checkResult = false;
            float maxDistance = attackRange;
            
            for (int index = 0; index < _playerCount; ++index)
            {
                var distance = FastDistance(transform.position, _players[index].transform.position);
                if (distance < attackRange)
                {
                    checkResult |= true;

                    if (distance < maxDistance)
                    {
                        maxDistance = distance;
                        _targetPlayerIndex = index;
                    }
                }
            }

            if (checkResult)
            {
                _targetRotation = (_players[_targetPlayerIndex].transform.position - transform.position).normalized;
                var targetAngle = Quaternion.FromToRotation(Vector3.forward, _targetRotation).eulerAngles.y;
                var mytAngle = Quaternion.FromToRotation(Vector3.forward, transform.rotation.eulerAngles).eulerAngles.y;
                
                _lookDirection = new Vector3(0, transform.rotation.eulerAngles.y + (targetAngle - mytAngle), 0);
                
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }
//
        /// <summary>
        /// 공격 행동을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartAttack()
        {           
            RotateRPC();
            
            networkAnimator.Animator.SetFloat(AttackBlend, ATTACK_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);
            _navMeshAgent.speed = 0.0f;
            
            // TODO : 피격 처리 해주는 코드 필요
            Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
            if (Runner.LagCompensation.Raycast(transform.position, transform.GetChild(0).forward, status.attackRange.Current, Runner.LocalPlayer, out var Hit))
            {
                PlayerStatus targetStatus;
                DebugManager.Log($"{Hit.GameObject.name} was raycast");
                // 공격 VFX
                if (Hit.GameObject.TryGetComponent(out targetStatus) || Hit.GameObject.transform.root.TryGetComponent(out targetStatus))
                {
                    DebugManager.Log($"{targetStatus.gameObject.name} was hit");
                    targetStatus.PlayerApplyDamage(status.CalDamage(), Object.Id);
                }
            }
            DebugManager.Log($"AttackRange : {status.attackRange.Current}");
            
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        #endregion

        #region Rush

        /// <summary>
        ///  돌진 행동이 실행중인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckRushAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        /// <summary>
        /// 돌진 행동의 공격범위에 플레이어가 있는지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckRushDistance()
        {
            bool checkResult = false;
            float minDistance = rushRange;
            
            for (int index = 0; index < _playerCount; ++index)
            {
                var distance = FastDistance(transform.position, _players[index].transform.position);
                if (distance < rushRange)
                {
                    checkResult |= true;

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _targetPlayerIndex = index;
                    }
                }
            }

            if (checkResult)
            {
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        /// <summary>
        /// 돌진 행돌을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartRush()
        {   
            networkAnimator.Animator.SetFloat(AttackBlend, RUSH_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);

            // 1. 고정 거리 돌진
            // Vector3 backVec = (_players[_targetPlayerIndex].transform.position - transform.position).normalized;
            // _navMeshAgent.speed = rushDistance / 3.0f;
            // _navMeshAgent.SetDestination(transform.position + backVec * rushDistance);
            
            // 2. player뒤까지 돌진
            Vector3 backVec = (_players[_targetPlayerIndex].transform.position - transform.position);
            
            _navMeshAgent.speed = (backVec.magnitude + (backVec.normalized.magnitude * rushDistance)) / 3.0f;   // player와 돼지의 거리 + 해당 벡터 * 10 만큼 더
            _navMeshAgent.SetDestination(transform.position + backVec + backVec.normalized * rushDistance);
            
            _durationTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        #endregion

        #region JumpAttack

        /// <summary>
        /// 점프 공격이 실행중인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckJumpAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 점프 공격의 공격범위안에 플레이어가 있는지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckJumpAttackDistance()
        {
            bool checkResult = false;
            float minDistance = 0.0f;
            
            for (int index = 0; index < _playerCount; ++index)
            {
                var distance = FastDistance(transform.position, _players[index].transform.position);
                if (distance < jumpRange)
                {
                    checkResult |= true;

                    if (distance > minDistance)
                    {
                        minDistance = distance;
                        _targetPlayerIndex = index;
                    }
                }
            }
            
            if (false == checkResult)
            {
                return INode.NodeState.Failure;
            }

            return INode.NodeState.Success;
        }

        #region jump
        
        /// <summary>
        /// 점프해서 플레이어를 향해 포물선 공격 OR 공중에서 코인 공격
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAction()
        {
            // 가장 먼 플레이어를 지정
            _navMeshAgent.speed = FastDistance(_players[_targetPlayerIndex].transform.position, transform.position) / 3.0f;
            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position);

            int type = Random.Range(1, 3); // 1 or 2
            
            if(type == 1)
                if (((float)(status.hp.Current) / status.hp.Max) <= 0.3f)
                    type = 2;
            
            networkAnimator.Animator.SetTrigger(Attack);
            networkAnimator.Animator.SetFloat(AttackBlend, JUMP_TYPE);
            
            StartCoroutine(JumpCoroutine(type));
            JumpUpRPC(type);
            
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }
        
        /// <summary>
        /// 바닥 균열 공격 OR 그냥 착지하는 점프 공격 패턴
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAttackAction()
        {
            int type;
            if ((float)(status.hp.Current) / status.hp.Max > 0.5f)
                type = 3;
            else
                type = 4;
            
            networkAnimator.Animator.SetTrigger(Attack);
            networkAnimator.Animator.SetFloat(AttackBlend, JUMP_TYPE);
            
            StartCoroutine(JumpCoroutine(type));
            JumpUpRPC(type);
            
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }

        IEnumerator JumpCoroutine(int type)
        {
            yield return new WaitForSeconds(1.0f);
            if(type == 1 || type == 3)
                PlayVFXRPC("PigGroundAttackRange");
            yield return new WaitForSeconds(2.0f);
            {
                _durationTime = _gameManager.PlayTimer;
                if (type == 2)
                {
                    networkAnimator.Animator.SetTrigger("tAttack");
                    networkAnimator.Animator.SetFloat("AttackBlend", 2);
                        
                    // TODO : Coin VFX
                    PlayVFXRPC("CoinMeteor_vfx");
                    PlayVFXRPC("PigMeteorRange");
                    StartCoroutine(MeteorApplyDamageCoroutine());
                        
                    while (IsAnimationRunning("Attack"))
                    {
                        yield return new WaitForSeconds(0.0f);
                    }
                }

                StartCoroutine(JumpDownCoroutine(type));
                JumpDownRPC(type);
                yield break;
            }
        }

        IEnumerator JumpDownCoroutine(float type)
            {
                yield return new WaitForSeconds(1.0f);
                {
                    if (type == 3 || type == 1)
                    {
                        PlayVFXRPC("GroundCrack_vfx");
                            
                        // TODO : 데미지 비율 상수로 조절하자
                        DebugManager.ToDo("데미지를 받을때 id받아오는 형식을 변수에 담아서 받아오자");
                        
                        status.ApplyDamageRPC((int)(status.hp.Current / 10.0f), gameObject.GetComponent<NetworkObject>().Id);
                        
                        // TODO : 천천히 오는 vfx와 부딪히는 판정으로 하고싶다
                        // TODO : 바로 적용 시키면 부자연스러움
                        for (int index = 0; index < _playerCount; ++index)
                        {
                            if (FastDistance(transform.position, _players[index].transform.position) < jumpAttackDamageRange)
                            {
                                //TODO : 데미지를 조정하자
                                // _players[index].GetComponent<StatusBase>().ApplyDamageRPC(status.CalDamage(), gameObject.GetComponent<NetworkObject>().Id);
                                _players[index].GetComponent<StatusBase>().PlayerApplyDamage(1, gameObject.GetComponent<NetworkObject>().Id);
                            }
                        }
                        CameraShakeRPC();
                    }
                    _durationTime = _gameManager.PlayTimer;
                    yield break;
                }
            }

        INode.NodeState StopJumpAttack()
        {
            GameObject targetGroundObject = transform.Find("GroundCrack_vfx").gameObject;
            GameObject targetMeteorObject = transform.Find("CoinMeteor_vfx").gameObject;

            if (true == targetGroundObject.activeSelf)
                StopVFXRPC("GroundCrack_vfx");
            else if (true == targetMeteorObject.activeSelf)
                StopVFXRPC("CoinMeteor_vfx");
            
            return INode.NodeState.Success;
        }
        
        #endregion

        #endregion

        #region Fart

        /// <summary>
        /// 방귀 행동이 실행중인지 판단하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckFartAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        /// <summary>
        /// 방귀 행동을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartFart()
        {
            networkAnimator.Animator.SetFloat(AttackBlend, FART_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);
            _navMeshAgent.SetDestination(transform.position);

            PlayVFXRPC("Fart_vfx");
            PlayVFXRPC("PigFartRange");
            StartCoroutine(FartApplyDamageCoroutine());
            //
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }
        IEnumerator FartApplyDamageCoroutine()
        {
            StatusBase[] _playerStatusBases;
            List<StatusBase> statusBases = new List<StatusBase>();
            foreach (var player in _players)
            {
                statusBases.Add(player.GetComponent<StatusBase>());
            }
            _playerStatusBases = statusBases.ToArray();
            
            yield return new WaitForSeconds(2.0f);
            for (int i = 0; i < 10; ++i)
            {
                yield return new WaitForSeconds(0.6f);
                for (int index = 0; index < _playerCount; ++index)
                {
                    var distance = FastDistance(transform.position, _players[index].transform.position);
                    
                    if (distance <= fartDamageRange)
                    {
                        //TODO : 데미지를 조정하자
                        _playerStatusBases[index].PlayerApplyDamage(1, gameObject.GetComponent<NetworkObject>().Id);
                    }
                    DebugManager.Log($"player to meteor distance : {distance}");
                }
            }
        }
        
        INode.NodeState StopFart()
        {
            StopVFXRPC("Fart_vfx");
            
            return INode.NodeState.Success;
        }
        
        #endregion

        #region Rest

        /// <summary>
        /// 휴식 행동이 실행중인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckRestAction()
        {
            if (IsAnimationRunning("Piggy_Rest"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 체력이 50%이하인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckRestHp()
        {
            if (status.hp.Current / status.hp.Max <= 0.5f) // 정밀한 검사 필요
            {
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        /// <summary>
        /// 휴식 행동을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartRest()
        {
            RestDownRPC(0);
            _navMeshAgent.SetDestination(transform.position);

            StartCoroutine(RestCoroutine());
            StartCoroutine(StartPressCoroutine());
            
            PlayVFXRPC("Rest_vfx");
            _durationTime = _gameManager.PlayTimer;
            
            return INode.NodeState.Success;
        }
        
        IEnumerator RestCoroutine()
        {
            int count = 0;//
            while (true)
            {
                _durationTime = _gameManager.PlayTimer;
                // TODO : 상수화 시키자
                RecoveryHPRPC();
                yield return new WaitForSeconds(0.5f);
                count++;
                // TODO: 상수화 시키자
                if (count >= 10)
                {
                    StopVFXRPC("Rest_vfx");
                    PressUpActionRPC();
                    RestUpRPC();
                    // TODO : 시간 동기화 필요
                    yield break;
                }
            }
        }

        IEnumerator StartPressCoroutine()
        {
            yield return new WaitForSeconds(2.0f);
            RestDownRPC(1);
            PressDownActionRPC();
        }
        
        #endregion

        #region CoinAttack

        /// <summary>
        /// 지상 코인공격 행동이 실행중인지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckCoinAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 지상 코인공격이 가능한 플레이어가 있는지 확인하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState CheckCoinAttackDistance()
        {
            bool checkResult = false;
            
            for (int index = 0; index < _playerCount; ++index)
            {
                var distance = FastDistance(transform.position, _players[index].transform.position);
                if (coinAtaackMinRange <= distance && distance <= coinAtaackMaxRange)
                {
                    checkResult |= true;
                }
            }

            if (checkResult)
            {
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        /// <summary>
        /// 지상 코인공격 행동을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartCoinAttack()
        {
            networkAnimator.Animator.SetTrigger(Attack);
            networkAnimator.Animator.SetFloat(AttackBlend, COIN_TYPE);
            
            PlayVFXRPC("CoinMeteor_vfx");
            PlayVFXRPC("PigMeteorRange");
            
            DebugManager.Log($"{FastDistance(transform.position, _players[0].transform.position)}");
            
            StartCoroutine(MeteorApplyDamageCoroutine());
            
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }

        IEnumerator MeteorApplyDamageCoroutine()
        {
            StatusBase[] _playerStatusBases;
            List<StatusBase> statusBases = new List<StatusBase>();
            foreach (var player in _players)
            {
                statusBases.Add(player.GetComponent<StatusBase>());
            }
            _playerStatusBases = statusBases.ToArray();
            
            for (int i = 0; i < 6; ++i)
            {
                yield return new WaitForSeconds(1.0f);
                for (int index = 0; index < _playerCount; ++index)
                {
                    var distance = FastDistance(new float3(transform.position.x, 0, transform.position.z), new float3(_players[index].transform.position.x, 0, _players[index].transform.position.z));
                    
                    if (coinAtaackMinRange <= distance && distance <= coinAtaackMaxRange)
                    {
                        //TODO : 데미지를 조정하자
                        _playerStatusBases[index].PlayerApplyDamage(1, gameObject.GetComponent<NetworkObject>().Id);
                    }
                    // DebugManager.Log($"player to meteor distance : {distance}");
                }
            }
        }

        INode.NodeState StopCoinAttack()
        {
            // StopVFX("CoinMeteor_vfx");
            StopVFXRPC("CoinMeteor_vfx");
            
            return INode.NodeState.Success;
        }
        #endregion
        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RotateRPC()
        {
            transform.GetChild(0).DORotate(_lookDirection, RotationDuration).SetEase(Ease.Linear);
        }

        #region Dfence

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ApplyDefenceRPC()
        {
            status.AddCondition(CrowdControl.DamageIgnore);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RemoveDefenceRPC()
        {
            status.DelCondition(CrowdControl.DamageIgnore);
        }

        #endregion
        
        #region JumpRPC
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void JumpUpRPC(int type)
        {
            DebugManager.ToDo("돼지 BT : status에서 속도를 받아오도록 수정");
            // TODO : transform.position.y + height로 수정
            transform.GetChild(0).DOMoveY(transform.position.y + jumpHeight, upTime).SetEase(Ease.OutCirc);
//
            if (type == 2)
            {
                GameObject targetObject = transform.Find("CoinMeteor_vfx").gameObject;

                if (targetObject != null)
                {
                    _visualEffect = targetObject.GetComponent<VisualEffect>();
                    _visualEffect.SetFloat("PigHeight", -jumpHeight);
                }
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void JumpDownRPC(int type)
        {
            DebugManager.ToDo("돼지 BT : status에서 속도를 받아오도록 수정");
            transform.GetChild(0).DOMoveY(transform.position.y, downTime).SetEase(Ease.InCirc);
            
            DebugManager.Log($"Y Position : {transform.position.y - jumpHeight}");
            if (type == 2)
            {
                GameObject targetObject = transform.Find("CoinMeteor_vfx").gameObject;

                if (targetObject != null)
                {
                    _visualEffect = targetObject.GetComponent<VisualEffect>();
                    _visualEffect.SetFloat("PigHeight", 0);
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void CameraShakeRPC()
        {
            for (int index = 0; index < _playerCount; ++index)
                _players[index].GetComponent<PlayerCameraController>().ShakeCamera(3,new Vector3(0, 1, 0), 5);
        }

        #endregion
        
        #region RestRPC

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RecoveryHPRPC()
        {
            status.hp.Current += (int)((status.hp.Max - status.hp.Current) / 0.05f);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PressDownActionRPC()
        {
            transform.DOScaleY(0.7f, 1.0f).SetEase(Ease.InOutQuad);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PressUpActionRPC()
        {
            transform.DOScaleY(1.0f, 1.0f).SetEase(Ease.InOutQuad);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RestDownRPC(int type)
        {
            if(type == 0)
                networkAnimator.Animator.SetTrigger(Rest);
            else
                transform.GetChild(0).DOMoveY(transform.position.y - 0.1f, 1.0f).SetEase(Ease.InOutQuad);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RestUpRPC()
        {
            networkAnimator.Animator.SetTrigger(EndRest);
            transform.GetChild(0).DOMoveY(transform.position.y + 0.1f, 1.0f).SetEase(Ease.InOutQuad);
        }

        #endregion
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PlayVFXRPC(string vfxName)
        {
            GameObject targetObject;
            
            // 이부분을 한번에 켜주면 최적화 됨
            if (vfxName == "PigFartRange" || vfxName == "PigGroundAttackRange" || vfxName == "PigMeteorRange")
                targetObject = transform.Find("Range_vfx").Find(vfxName).gameObject;
            else
                targetObject = transform.Find(vfxName).gameObject;

            if (targetObject != null)
            {
                _visualEffect = targetObject.GetComponent<VisualEffect>();

                if (_visualEffect != null)
                {
                    if(!targetObject.activeSelf)
                        targetObject.gameObject.SetActive(true);
                    _visualEffect.SendEvent("OnPlay");
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void StopVFXRPC(string vfxName)
        {
            GameObject targetObject = transform.Find(vfxName).gameObject;
            DebugManager.Log($"{FastDistance(_players[0].transform.position , transform.position)}");
            
            if (targetObject != null)
            {
                _visualEffect = targetObject.GetComponent<VisualEffect>();
                if (_visualEffect != null)
                {
                    _visualEffect.SendEvent("OffPlay");
                }
            }
        }

        #endregion
    }
}