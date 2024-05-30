using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorTree.Base;
using Data;
using DG.Tweening;
using GamePlay;
using Status;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class NavTest : MonoBehaviour
{
    // [SerializeField] private GameObject player;
    // [SerializeField] private Animator animator;
    // private NavMeshAgent _agent;
    //
    // NavMeshPath _path;
    // private float _height;
    //
    // private float _startTime;
    //
    // public float risingSpeed = 1.0f;
    // public float downSpeed = 1.0f;
    //
    // public int attackType = 2;
    // // 1 : 점프 공격
    // // 2 : 점프 코인공격
    //
    // void Start()
    // {
    //     _height = 0.0f;
    //     _path = new NavMeshPath();
    //     _agent = GetComponent<NavMeshAgent>();
    //     animator = GetComponentInChildren<Animator>();
    //
    //     // animator.SetFloat("AttackBlend", 3);
    //     // animator.SetTrigger("tAttack");
    //
    //     if (attackType == 1)
    //     {
    //         _agent.speed = FastDistance(player.transform.position, transform.position) / 3.0f;
    //         _agent.SetDestination(player.transform.position);
    //         StartCoroutine(JumpUpCoroutine(risingSpeed, 0.0f, 1));
    //     }
    //     else if (attackType == 2)
    //     {
    //         StartCoroutine(JumpUpCoroutine(risingSpeed, 0.0f, 2));
    //     }
    // }
    //
    // private void Update()
    // {
    //     transform.position = new Vector3(transform.position.x, _height, transform.position.z);
    // }
    //
    // IEnumerator JumpUpCoroutine(float risingSpeed, float waitTime, int type)
    // {
    //     _startTime = 0;
    //     
    //     while (true)
    //     {
    //         _startTime += Time.deltaTime;
    //         _height += risingSpeed * Time.deltaTime * (11 - _height);
    //
    //         transform.position = new Vector3(transform.position.x, _height, transform.position.z);
    //
    //         yield return new WaitForSeconds(waitTime);
    //
    //         if (_height >= 10.0f)
    //         {
    //             if(type == 2)
    //             {
    //                 animator.SetLayerWeight(1, 1.0f);
    //                 animator.SetTrigger("tCoin");
    //                 animator.SetTrigger("tAttack");
    //                 animator.SetFloat("AttackBlend", 2);
    //                 yield return new WaitForSeconds(5.0f);
    //                 animator.SetLayerWeight(1, 0.0f);
    //             }
    //             StartCoroutine(JumpDownCoroutine(downSpeed));
    //             yield break;
    //         }
    //     }
    // }
    //
    // IEnumerator JumpDownCoroutine(float downSpeed)
    // {
    //     while (true)
    //     {
    //         _startTime += Time.deltaTime;
    //         _height -= downSpeed * Time.deltaTime * (11 - _height);
    //         transform.position = new Vector3(transform.position.x, _height, transform.position.z);
    //         yield return new WaitForSeconds(0.0f);
    //
    //         if (_height < 0.0f)
    //         {
    //             _height = 0.0f;
    //             yield break;
    //         }
    //     }
    // }
    //
    // public static float FastDistance(float3 pointA, float3 pointB)
    // {
    //     return math.distance(pointA, pointB);
    // }
    
    #region Component
        
        private BehaviorTreeRunner _btRunner;
        private VisualEffect _visualEffect;
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;

        private UserData _userData;
        [SerializeField]private GameObject[] _players;

        //[field:SerializeField] // 프로퍼티도 인스펙터에서 보여줌

        [Header("공격 범위")]
        [SerializeField] private float duration = 3; // 발차기 감지 범위
        [SerializeField] private float strength = 1;
        [SerializeField] private int vibrato = 5;
        [SerializeField] private float randomness = 0;
        
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
        [SerializeField] private float coinAtaackMinRange = 10; // 코인 공격 최소 감지 범위
        [SerializeField] private float coinAtaackMaxRange = 100; // 코인 공격 최대 감지 범위

        private float jumpHeight = 30;
        private float _height = 0;

        private static readonly int Walk = Animator.StringToHash("isWalk");

        private static readonly int Dead = Animator.StringToHash("isDead");
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
            // _btRunner = new BehaviorTreeRunner(SettingBT());
            _visualEffect = GetComponentInChildren<VisualEffect>();
            _animator = GetComponentInChildren<Animator>();
            if(TryGetComponent(out _navMeshAgent)== false) _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            _playerCount = 1;

            attackRange = 10;
            rushRange = 100;
            coinAtaackMinRange = 30;
            coinAtaackMaxRange = 150;

            isDead = false;

            _animator.SetFloat(AttackBlend, 0);
            // _animator.SetBool(Walk, true);

            // StartCoroutine(WalkCoroutine(0.5f));
            // StartCoroutine(DieCoroutine(0.5f));

            // transform.DOScaleY(0.8f, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InCirc);// 점프할때ㅐ 
            Transform tras = transform.GetChild(0);

            tras.DOShakeRotation(duration,new Vector3(0, strength, 0), vibrato, randomness);
        }
        

        public void FixedUpdate()
        {
            if (!isDead)
                _btRunner.Operator();
        }

        #region BT
        
        // IEnumerator DieCoroutine(float waitTime)
        // {
        //     while (true)
        //     {
        //         if (_status.IsDie)
        //         {
        //             _animator.SetTrigger(Dead);
        //             isDead = true;
        //             yield break;
        //         }
        //
        //         yield return new WaitForSeconds(waitTime);
        //     }
        // }
        
        IEnumerator WalkCoroutine(float waitTime)
        {
            while (true)
            {
                if (IsAnimationRunning("piggy_walk"))
                {
                    if (FastDistance(transform.position, _players[0].transform.position) > 10.0f)
                    {
                        _navMeshAgent.speed = 1;
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

        INode SettingBT()
        {
            return new SequenceNode
            (
                new SelectorNode
                (
                    true,   // test : 원래는 false
                    new SequenceNode
                    ( // Deffence
                        // new ActionNode(CheckMoreHp),
                        new ActionNode(StartDefence),
                        new ActionNode(TermFuction)
                    ),
                    new SequenceNode
                    ( // Run
                        new SequenceNode(
                            new ActionNode(StartRun),
                            new ActionNode(TermFuction),
                            new ActionNode(StopRun)
                        ),
                        new SequenceNode(
                            new ActionNode(CheckJumpAttackAction),
                            // new ActionNode(CheckJumpAttackDistance),
                            new ActionNode(StartJumpAction),
                            new ActionNode(TermFuction),
                            new ActionNode(StopJumpAttack)
                        )
                    )
                ),
                new SelectorNode(
                    false,
                    new SequenceNode
                    (
                        new ActionNode(CheckAttackAction), // Kick
                        // new ActionNode(CheckAttackDistance),
                        // new ActionNode(StartRotate),
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
                        new SequenceNode(
                            new ActionNode(CheckRushAction), // Rush
                            // new ActionNode(CheckRushDistance),
                            new ActionNode(StartRush),
                            new ActionNode(TermFuction)
                        ),
                        new SequenceNode
                        (
                            new ActionNode(CheckJumpAttackAction), // JumpAttack
                            new ActionNode(StartJumpAttackAction),
                            new ActionNode(TermFuction),
                            new ActionNode(StopJumpAttack)
                        )
                    ),
                    new SequenceNode
                    (
                        new ActionNode(CheckFartAction), // fart
                        new ActionNode(StartFart),
                        new ActionNode(TermFuction)
                    )
                ),
                new SequenceNode
                ( // CoinAttack
                    new ActionNode(CheckCoinAttackAction),
                    // new ActionNode(CheckCoinAttackDistance),
                    new ActionNode(StartCoinAttack),
                    new ActionNode(TermFuction)
                ),
                new SequenceNode
                ( // take a rest
                    // new ActionNode(CheckRestAction),
                    // new ActionNode(CheckRestHp),
                    new ActionNode(StartRest),
                    new ActionNode(TermFuction)
                )
            );
        }

        INode TestSettingBT()
        {
            return new SequenceNode
            (
                new ActionNode(CheckRestAction),
                new ActionNode(StartRest),
                new ActionNode(TermFuction)
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

        #region VFX Function

        void PlayVFX(string vfxName)
        {
            var target = transform.Find(vfxName);

            if (target != null)
            {
                _visualEffect = target.gameObject.GetComponent<VisualEffect>();
                if (vfxName == "GroundCrack_vfx" || vfxName == "NormalAttack_vfx")
                {
                    target.gameObject.SetActive(true);
                    _visualEffect.SendEvent("OnPlay");
                }
                else if (_visualEffect != null)
                {
                    target.gameObject.SetActive(true);
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
                    targetObject.SetActive(false);
                    _visualEffect.Stop();
                }
            }
        }

        #endregion

        INode.NodeState TermFuction()
        {
            if (_durationTime > 5.0f)
            {
                // TODO : 일단 _visualEffect에 참조 되기에 문제는 없을것 같은데 문제가 있을시에 함수를 써서 멈추자
                // _visualEffect.Stop();
                // StopVFX();
                // _animator.Play("piggy_walk");

                _durationTime = 0.0f;

                return INode.NodeState.Success;
            }
            return INode.NodeState.Running;
        }

        IEnumerator TimeCoroutine()
        {
                yield return new WaitForSeconds(5.0f);
                
                _durationTime = 6.0f;
                yield break;
        }


        INode.NodeState SuccessFunction()
        {
            return INode.NodeState.Success;
        }
        
        INode.NodeState StartWalk()
        {
            //TODO : 플레이어의 거리를 측정하거나 랜덤한 좌표에 가는 코드를 구현해야한다
            // 그러려면 target을 가까운 플레이어나 유리한 위치를 찾는 알고리즘이 필요하다.
            // GameObject target = 
            // _agent.SetDestination(target.position);

            return INode.NodeState.Success;
        }

        #region 방어 OR 도주

        #region HPCheck

        INode.NodeState CheckMoreHp()
        {
            // if (_status.hp.Current / _status.hp.Max > 0.5f)
            // {
            //     return INode.NodeState.Success;
            // }
            return INode.NodeState.Success;
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

            Debug.Log("Defence");
            // _visualEffect.Play();
            PlayVFX("shield_vfx");
            _animator.SetTrigger(Defence);
            _navMeshAgent.SetDestination(transform.position);    
            _durationTime = 0;

            StartCoroutine(TimeCoroutine());

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
            
            //TODO: 방향 설정하는 코드 필요
            Debug.Log("Run");
            _animator.SetFloat(AttackBlend, RUSH_TYPE);
            _animator.SetTrigger(Attack);
            _durationTime = 0;
            StartCoroutine(TimeCoroutine());
            return INode.NodeState.Success;
        }

        INode.NodeState StopRun()
        {
            // TODO : 방향 다시 설정
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

        /// <summary>
        /// 돼지저금통이 공격을 하는중인지 판단하는 함수
        /// </summary>
        INode.NodeState CheckAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

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

                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRotate()
        {
            _timePassed += Time.deltaTime;

            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _timePassed / RotationDuration);

            if (_timePassed >= RotationDuration)
            {
                _timePassed = 0.0f;
                return INode.NodeState.Success;
            }

            return INode.NodeState.Running;
        }

        INode.NodeState StartAttack()
        {
            _animator.SetFloat(AttackBlend, ATTACK_TYPE);
            _animator.SetTrigger(Attack);
            _navMeshAgent.speed = 0.0f;
            _durationTime = 0;
            StartCoroutine(TimeCoroutine());
            
            PlayVFX("NormalAttack_vfx");
            
            return INode.NodeState.Success;
        }

        #endregion

        #region Rush

        INode.NodeState CheckRushAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        // TODO : 러쉬의 범위를 제안하면 점프 공격의 패턴이 거의 안나올 가능성이 있기에 러쉬의 범위제한을 없애는 방향으로 가거나 점프공격을 포물선으로 움직이게 하면 되지 않을까
        // 거리 체크 
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
                Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
                _targetRotation = Quaternion.LookRotation(targetDirection);

                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRush()
        {   
            Debug.Log("Rush");
            _animator.SetFloat(AttackBlend, RUSH_TYPE);
            _animator.SetTrigger(Attack);

            // TODO : player가 쳐다보고있는 방향이 아닌 돼지와 player의 벡터만큼 뒤로 가자
            Vector3 backVec = math.normalize(_players[_targetPlayerIndex].transform.position - transform.position);
            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position + backVec * 5);
            
            // TODO : 돌진 속도를 Status로 조절해주기 조절해주기
            _navMeshAgent.speed = 10.0f;

            _durationTime = 0;
            StartCoroutine(TimeCoroutine());
            return INode.NodeState.Success;
        }

        #endregion

        #region JumpAttack

        INode.NodeState CheckJumpAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

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

            // TODO : 점프 공격의 거리도 정하자
            
            if (FastDistance(_players[_targetPlayerIndex].transform.position, transform.position) < rushRange / 3.0f)
            {
                return INode.NodeState.Failure;
            }
            
            Vector3 targetDirection = _players[_targetPlayerIndex].transform.position - transform.position;
            _targetRotation = Quaternion.LookRotation(targetDirection);

            _rotationlastTime = 0;

            return INode.NodeState.Success;
        }

        #region jump
        
        /// <summary>
        /// 점프해서 플레이어를 향해 포물선 공격 OR 공중에서 코인 공격
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAction()
        {
            Debug.Log("JumpAttack1");
            _animator.SetTrigger(Attack);
            _animator.SetFloat(AttackBlend, JUMP_TYPE);
            
            // 가장 먼 플레이어를 지정
            _navMeshAgent.speed = FastDistance(_players[_targetPlayerIndex].transform.position, transform.position) / 3.0f;
            _navMeshAgent.SetDestination(_players[_targetPlayerIndex].transform.position);

            int type = Random.Range(1, 3); // 1 or 2
            
            StartCoroutine(JumpCoroutine(1.0f, 3.0f, type));
            
            return INode.NodeState.Success;
        }
        
        /// <summary>
        /// 바닥 균열 공격 OR 그냥 착지하는 점프 공격 패턴
        /// </summary>
        /// <returns></returns>
        INode.NodeState StartJumpAttackAction()
        {
            Debug.Log("JumpAttack2");
            _animator.SetTrigger(Attack);
            _animator.SetFloat(AttackBlend, JUMP_TYPE);
            int type;
            // if (_status.hp.Current / _status.hp.Max > 0.5f)
                type = 3;
            // else
            //     type = 4;
            
            StartCoroutine(JumpCoroutine(1.0f, 3.0f, type));
            
            return INode.NodeState.Success;
        }

        IEnumerator JumpCoroutine(float risingSpeed, float downSpeed, int type)
        {
            while (true)
            {
                _height += risingSpeed * Time.deltaTime * (11 - _height);

                transform.position = new Vector3(transform.position.x, _height, transform.position.z);

                yield return new WaitForSeconds(0.0f);

                if (_height >= 10.0f)
                {
                    if (type == 2)
                    {
                        _animator.SetTrigger("tAttack");
                        _animator.SetFloat("AttackBlend", 2);
                        while (IsAnimationRunning("Attack"))
                        {
                            transform.position = new Vector3(transform.position.x, _height, transform.position.z);
                            yield return new WaitForSeconds(0.0f);
                        }
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
                    _height -= downSpeed * Time.deltaTime * (11 - _height);
                    transform.position = new Vector3(transform.position.x, _height, transform.position.z);
                    yield return new WaitForSeconds(0.0f);

                    if (_height < 0.0f)
                    {
                        if (type == 3 || type == 1)
                        {
                            PlayVFX("GroundCrack_vfx");
                            
                            
                            // 데미지 입히기
                            // TODO : 데미지 비율 상수로 조절하자
                            // _status.hp.Current -= (int)(_status.hp.Max / 0.05f);
                        }
                        _height = 0.0f;
                        _durationTime = 0;
                        StartCoroutine(TimeCoroutine());   
                        yield break;
                    }
                }
            }
        
            INode.NodeState StopJumpAttack()
            {
                GameObject targetObject = transform.Find("GroundCrack_vfx").gameObject;
            
                if(true == targetObject.activeSelf)
                    StopVFX("GroundCrack_vfx");
            
                return INode.NodeState.Success;
            }
        
        #endregion

        #endregion

        #region Fart

        INode.NodeState CheckFartAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        INode.NodeState StartFart()
        {
            Debug.Log("Fart");
            _animator.SetFloat(AttackBlend, FART_TYPE);
            _animator.SetTrigger(Attack);
            _navMeshAgent.SetDestination(transform.position);
            
            // 분진 VFX
            // PlayVFX("");

            _durationTime = 0;
            StartCoroutine(TimeCoroutine());
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
            // if (_status.hp.Current / _status.hp.Max <= 0.5f) // 정밀한 검사 필요
            // {
            //     return INode.NodeState.Success;
            // }

            return INode.NodeState.Failure;
        }

        INode.NodeState StartRest()
        {
            Debug.Log("Rest");
            _animator.SetTrigger(Rest);
            _navMeshAgent.SetDestination(transform.position);

            _durationTime = 0;
            StartCoroutine(RestCoroutine());

            return INode.NodeState.Success;
        }

        IEnumerator RestCoroutine()
        {
            int count = 0;
            while (true)
            {
                // TODO : 상수화 시키자
                // _status.hp.Current += (int)((_status.hp.Max - _status.hp.Current) / 0.05f);
                yield return new WaitForSeconds(0.5f);
                count++;
                // TODO: 상수화 시키자
                if (count >= 10)
                {
                    _durationTime = 6.0f;
                    yield break;
                }
            }
        }

        #endregion

        #region CoinAttack

        INode.NodeState CheckCoinAttackAction()
        {
            if (IsAnimationRunning("Attack"))
            {
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

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

        INode.NodeState StartCoinAttack()
        {
            Debug.Log("Coin");
            _animator.SetTrigger(Attack);
            _animator.SetFloat(AttackBlend, COIN_TYPE);

            // TODO : VFX를 받아와서 실행하고, 매개변수로 자식 객체의 이름를 받는 함수구현

            _durationTime = 0;
            StartCoroutine(TimeCoroutine());

            return INode.NodeState.Success;
        }

        #endregion
        #endregion
}
