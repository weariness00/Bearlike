using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using Data;
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

        private const float RotationDuration = 1.0f;

        private Quaternion _targetRotation; // 목표 회전값
        private float _timePassed = 0f; // 회전 보간에 사용될 시간 변수

        private float _rotationlastTime;

        #endregion

        private bool isDead = false;

        [Header("공격 범위")]
        [SerializeField] private float attackRange = 10; // 발차기 감지 범위
        [SerializeField] private float rushRange = 100; // 돌진 감지 범위
        [SerializeField] private float jumpRange = 50;
        [SerializeField] private float coinAtaackMinRange = 30; // 코인 공격 최소 감지 범위
        [SerializeField] private float coinAtaackMaxRange = 100; // 코인 공격 최대 감지 범위

        [Header("상하 속도")] 
        [SerializeField] private float upSpeed = 1.0f;
        [SerializeField] private float downSpeed = 3.0f;
        
        [Header("이동 정도")] 
        [SerializeField]private float runDistance = 3.0f;
        [SerializeField] private float rushDistance = 5.0f;
        [SerializeField]private float jumpHeight = 10.0f;
        
        private float _height = 0;
        private float _delayTime = 5.0f;

        private static readonly int Walk = Animator.StringToHash("tWalk");
        private static readonly int Dead = Animator.StringToHash("tDead");
        private static readonly int Rest = Animator.StringToHash("tRest");
        private static readonly int Defence = Animator.StringToHash("tDefence");
        private static readonly int Attack = Animator.StringToHash("tAttack");

        private static readonly int AttackBlend = Animator.StringToHash("AttackBlend");

        private const int ATTACK_TYPE = 0;
        private const int FART_TYPE = 1;
        private const int COIN_TYPE = 2;
        private const int JUMP_TYPE = 3;
        private const int RUSH_TYPE = 4;

        #endregion

        private void Awake()
        {
            base.Awake();
            _btRunner = new BehaviorTreeRunner(SettingBT());
            _visualEffect = GetComponentInChildren<VisualEffect>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.enabled = false;
        }

        private void Start()
        {
            base.Start();
            _gameManager = GameManager.Instance; 
            _playerCount = _gameManager.AlivePlayerCount;
            
            isDead = false;

            networkAnimator.Animator.SetFloat(AttackBlend, 0);
            StartCoroutine(DieCoroutine(1.0f));
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
            if (!isDead)
                _btRunner.Operator();
        }

        #region BT
        
        IEnumerator DieCoroutine(float waitTime)
        {
            while (true)
            {
                if (status.IsDie)
                {
                    networkAnimator.Animator.SetTrigger(Dead);
                    isDead = true;
                    yield break;
                }

                yield return new WaitForSeconds(waitTime);
            }
        }

        #region BT

        INode SettingBT()
        {
            return new SelectorNode
            (
                true,
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
                            // new ActionNode(CheckJumpAttackDistance),
                            new ActionNode(StartJumpAction),
                            new ActionNode(TermFuction),
                            new ActionNode(StopJumpAttack)
                        )
                    )
                ),
                // new SelectorNode(
                //     false,
                    new SequenceNode
                    (   // Kick
                        new ActionNode(CheckAttackAction),
                        new ActionNode(CheckAttackDistance),
                        new ActionNode(StartRotate),
                        new ActionNode(StartAttack),
                        new ActionNode(TermFuction)
                    ),
                //     new ActionNode(SuccessFunction)
                // ),
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
                new SequenceNode
                (   // fart
                    new ActionNode(CheckFartAction), 
                    new ActionNode(StartFart),
                    new ActionNode(TermFuction),
                    new ActionNode(StopFart)
                )
            );
        }

        #endregion

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

        #region VFX Function

        void PlayVFX(string vfxName)
        {
            GameObject targetObject = transform.Find(vfxName).gameObject;

            if (targetObject != null)
            {
                _visualEffect = targetObject.GetComponent<VisualEffect>();

                if (_visualEffect != null)
                {
                    if (vfxName == "GroundCrack_vfx" || vfxName == "Fart_vfx")
                    {
                        targetObject.gameObject.SetActive(true);
                        _visualEffect.SendEvent("OnPlay");
                    }
                    else
                    {
                        targetObject.SetActive(true);
                        _visualEffect.Play();
                    }
                }
            }
            else
            {
                DebugManager.Log($"{vfxName}은 존재하지 않는 VFX이름입니다.");
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
                    targetObject.SetActive(false);
                }
            }
        }

        #endregion

        #region AdditionalNode

        /// <summary>
        /// delayTime동안 지연시간을 부여하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState TermFuction()
        {
            if (_gameManager.PlayTimer - _durationTime > _delayTime)
            {
                // TODO : 일단 _visualEffect에 참조 되기에 문제는 없을것 같은데 문제가 있을시에 함수를 써서 멈추자
                // _visualEffect.Stop();
                // StopVFX();
                // networkAnimator.Animator.Play("piggy_walk");

                return INode.NodeState.Success;
            }

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
            int minHp = Int32.MaxValue;
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
            // 무게 중심으로 이동?
            // 아니면 플레이어에게서 멀어지는 방향으로 이동?
                
            _durationTime = _gameManager.PlayTimer;
                
            DebugManager.Log("Pig Walk");
                
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
            if (status.hp.Current / status.hp.Max > 0.5f)
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
            
            // _visualEffect.Play();
            PlayVFX("Shield_vfx");
            networkAnimator.Animator.SetTrigger(Defence);
            _navMeshAgent.SetDestination(transform.position);
            
            status.AddCondition(CrowdControl.Defence);
            
            _durationTime = _gameManager.PlayTimer;

            DebugManager.Log("Pig Defence");
            return INode.NodeState.Success;
        }

        /// <summary>
        /// Defence 동작을 그만하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StopDefence()
        {
            status.DelCondition(CrowdControl.Defence);
            StopVFX("Shield_vfx");
            
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
            }
            centroid.x /= _playerCount;
            centroid.y /= _playerCount;

            var position1 = transform.position;
            var direction = position1 - centroid;
            
            networkAnimator.Animator.SetFloat(AttackBlend, RUSH_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);
            
            _navMeshAgent.speed = direction.magnitude * runDistance / 3.0f;   // animation의 길이가 3초
            _navMeshAgent.SetDestination(position1 + direction * runDistance);

            _durationTime = _gameManager.PlayTimer;

            DebugManager.Log("Pig Run");
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
                Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
                _targetRotation = Quaternion.LookRotation(targetDirection);

                _rotationlastTime = _gameManager.PlayTimer;

                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        /// <summary>
        /// 목표로 하는 플레이어를 바라보도록 회전하는 노드
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 공격 행동을 실행하는 노드
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartAttack()
        {
            networkAnimator.Animator.SetFloat(AttackBlend, ATTACK_TYPE);
            networkAnimator.Animator.SetTrigger(Attack);
            _navMeshAgent.speed = 0.0f;
            
            // TODO : 피격 처리 해주는 코드 필요
            Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;

            if (targetDirection.magnitude <= attackRange)
            {
                
            }
            
            _durationTime = _gameManager.PlayTimer;

            DebugManager.Log("Pig Attack");
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
        // TODO : 러쉬의 범위를 제안하면 점프 공격의 패턴이 거의 안나올 가능성이 있기에 러쉬의 범위제한을 없애는 방향으로 가거나 점프공격을 포물선으로 움직이게 하면 되지 않을까
        INode.NodeState CheckRushDistance()
        {
            bool checkResult = false;
            float maxDistance = rushRange;
            
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

            Vector3 backVec = math.normalize(_players[_targetPlayerIndex].transform.position - transform.position);

            _navMeshAgent.speed = FastDistance(transform.position, _players[_targetPlayerIndex].transform.position + backVec * rushDistance) / 3.0f;
            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position + backVec * rushDistance);
            
            _durationTime = _gameManager.PlayTimer;

            DebugManager.Log("Pig Rush");
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
                if (distance < attackRange)
                {
                    checkResult |= true;

                    if (distance > minDistance)
                    {
                        minDistance = distance;
                        _targetPlayerIndex = index;
                    }
                }
            }
            
            if (FastDistance(_players[_targetPlayerIndex].transform.position, transform.position) < jumpRange)
            {
                return INode.NodeState.Failure;
            }

            _rotationlastTime = _gameManager.PlayTimer;

            return INode.NodeState.Success;
        }

        #region jump
        
        /// <summary>
        /// 점프해서 플레이어를 향해 포물선 공격 OR 공중에서 코인 공격
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAction()
        {
            networkAnimator.Animator.SetTrigger(Attack);
            networkAnimator.Animator.SetFloat(AttackBlend, JUMP_TYPE);
            
            // 가장 먼 플레이어를 지정
            _navMeshAgent.speed = FastDistance(_players[_targetPlayerIndex].transform.position, transform.position) / 3.0f;
            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position);

            int type = Random.Range(1, 3); // 1 or 2
            
            DebugManager.ToDo("돼지 BT : status에서 속도를 받아오도록 수정");
            StartCoroutine(JumpCoroutine(upSpeed, downSpeed, type));
            
            _durationTime = _gameManager.PlayTimer;
            DebugManager.Log($"Pig Jump{type}");
            return INode.NodeState.Success;
        }
        
        /// <summary>
        /// 바닥 균열 공격 OR 그냥 착지하는 점프 공격 패턴
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAttackAction()
        {
            networkAnimator.Animator.SetTrigger(Attack);
            networkAnimator.Animator.SetFloat(AttackBlend, JUMP_TYPE);
            int type;
            if (status.hp.Current / status.hp.Max > 0.5f)
                type = 3;
            else
                type = 4;
            
            DebugManager.ToDo("돼지 BT : status에서 속도를 받아오도록 수정");
            StartCoroutine(JumpCoroutine(upSpeed, downSpeed, type));
            
            _durationTime = _gameManager.PlayTimer;
            DebugManager.Log($"Pig Jump{type}");
            return INode.NodeState.Success;
        }

        IEnumerator JumpCoroutine(float risingSpeed, float downSpeed, int type)
        {
            while (true)
            {
                _height += risingSpeed * Runner.DeltaTime * (jumpHeight + 1 - _height);
                var transform1 = transform;
                transform.position = new Vector3(transform1.position.x, transform1.position.y + _height, transform1.position.z);

                yield return new WaitForSeconds(0.0f);

                if (_height >= jumpHeight)
                {
                    _durationTime = _gameManager.PlayTimer;
                    if (type == 2)
                    {
                        networkAnimator.Animator.SetTrigger("tAttack");
                        networkAnimator.Animator.SetFloat("AttackBlend", 2);
                        
                        // TODO : Coin VFX
                        //PlayVFX("CoinUp");
                        PlayVFX("CoinMeteor_vfx");
                        
                        while (IsAnimationRunning("Attack"))
                        {
                            transform.position = new Vector3(transform1.position.x, transform1.position.y + _height, transform1.position.z);
                            yield return new WaitForSeconds(0.0f);
                        }
                        
                        // TODO : Coin object active true
                    }

                    StartCoroutine(JumpDownCoroutine(downSpeed, type));
                    yield break;
                }
            }
        }

        IEnumerator JumpDownCoroutine(float downSpeed, float type)
            {
                while (true)
                {
                    _height -= downSpeed * Runner.DeltaTime * (jumpHeight + 1 - _height);                
                    var transform1 = transform;
                    transform.position = new Vector3(transform1.position.x, transform1.position.y + _height, transform1.position.z);
                    yield return new WaitForSeconds(0.0f);

                    if (_height < 0.0f)
                    {
                        if (type == 3 || type == 1)
                        {
                            // VFX실행
                            PlayVFX("GroundCrack_vfx");
                            
                            // 데미지 입히기
                            // TODO : 데미지 비율 상수로 조절하자
                            // if(type == 3)
                            //     status.hp.Current -= (int)(status.hp.Max / 0.05f);
                        }
                        
                        _height = 0.0f;
                        _durationTime = _gameManager.PlayTimer;
                        yield break;
                    }
                }
            }

        INode.NodeState StopJumpAttack()
        {
            GameObject targetGroundObject = transform.Find("GroundCrack_vfx").gameObject;
            GameObject targetMeteorObject = transform.Find("CoinMeteor_vfx").gameObject;
            
            if(true == targetGroundObject.activeSelf)
                StopVFX("GroundCrack_vfx");
            else if(true == targetMeteorObject.activeSelf)
                StopVFX("CoinMeteor_vfx");
            
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
            
            // 분진 VFX
            PlayVFX("Fart_vfx");

            DebugManager.Log($"Pig Fart");
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }

        INode.NodeState StopFart()
        {
            StopVFX("Fart_vfx");

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
            networkAnimator.Animator.SetTrigger(Rest);
            _navMeshAgent.SetDestination(transform.position);

            StartCoroutine(RestCoroutine());
            DebugManager.Log($"Pig Rest");
            _durationTime = _gameManager.PlayTimer;
            return INode.NodeState.Success;
        }
        
        IEnumerator RestCoroutine()
        {
            int count = 0;
            while (true)
            {
                // TODO : 상수화 시키자
                status.hp.Current += (int)((status.hp.Max - status.hp.Current) / 0.05f);
                yield return new WaitForSeconds(0.5f);
                count++;
                // TODO: 상수화 시키자
                if (count >= 10)
                {
                    // TODO : 시간 동기화 필요
                    yield break;
                }
            }
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
                // 동전 유무 판단도 넣을까?
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
            
            PlayVFX("CoinMeteor_vfx");
            
            _durationTime = _gameManager.PlayTimer;
            DebugManager.Log($"Pig Coin");
            return INode.NodeState.Success;
        }

        INode.NodeState StopCoinAttack()
        {
            StopVFX("CoinMeteor_vfx");

            return INode.NodeState.Success;
        }
        #endregion
        #endregion
    }
}