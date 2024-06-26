using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using DG.Tweening;
using Fusion;
using Sound;
using Status;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using DebugManager = Manager.DebugManager;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    public class BoxJester : MonsterBase
    {
        #region Property
        
        [Header("Animator")]
        [SerializeField] private BoxJesterAnimator animator;
        
        [Header("Teleport Properties")]
        [SerializeField] private Transform[] tpPlaces;

        [Header("HandAttack Properties")] 
        [SerializeField] private GameObject[] hands;
        
        [Header("VFX Properties")]
        [SerializeField] private VisualEffect tpEffect;
        [SerializeField] private VisualEffect darknessAttackEffect;

        [Header("Effect")] 
        [SerializeField] private Material bloodShieldMat;
        
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        
        // public SoundBox soundBox;
        private BehaviorTreeRunner _behaviorTreeRunner;
        private GameObject[] _players;
        private GameObject[] _masks;
        
        enum MaskType
        {
            Smile = 0,
            Cry = 1,
            Angry = 2
        }
        
        private MaskType _maskType = MaskType.Smile;
        private int _tpPlaceIndex = 0;
        private int _shieldType;
        private bool _animationing = false;
        
        #endregion


        #region Unity Event Function

        void Awake()
        {
            base.Awake();
            
            animator = GetComponentInChildren<BoxJesterAnimator>();
            
            // hands = new GameObject[2];
            // hands[0] = transform.Find("")
            
            // tpEffect.SetFloat("Time", animator.tptClip.length);

            var bloodShieldRenderer = transform.Find("ShieldEffect").GetComponent<Renderer>();
            bloodShieldMat = bloodShieldRenderer.material;
            
            // Mask 대입
            var boxJester = transform.Find("Clown");
            
            _masks = new GameObject[3];
            _masks[0] = boxJester.Find("Smile_Face").gameObject;
            _masks[1] = boxJester.Find("Sad_Face").gameObject;
            _masks[2] = boxJester.Find("Angry_Face").gameObject;
        }
        
        #endregion

        #region Member Function

        public override void Spawned()
        {
            base.Spawned();
            tpEffect.SendEvent("StopPlay");

            // Transform rootTrans = transform.root.Find("TPPosition"); // pool에 들어가는 경우
            Transform rootTrans = GameObject.Find("Boss Stage").transform.Find("TPPosition"); // 안들어가는 경우

            if (rootTrans != null)
            {
                tpPlaces = new Transform[rootTrans.childCount];

                for (int i = 0; i < rootTrans.childCount; ++i)
                {
                    DebugManager.Log($"tpPlaces[i] : {tpPlaces[i]}, rootTrans.GetChild(i) : {rootTrans.GetChild(i)}");
                    tpPlaces[i] = rootTrans.GetChild(i);
                }
            }
            else
            {
                DebugManager.LogError($"BoxJester의 TPPosition이 NULL입니다.");
            }
            
            // InGame Player 대입
            List<GameObject> playerObjects = new List<GameObject>();
            foreach (var playerRef in Runner.ActivePlayers.ToArray())
            {
                playerObjects.Add(Runner.GetPlayerObject(playerRef).gameObject);
            }
            _players = playerObjects.ToArray();
        }
        
        
        #endregion

        #region BT Function
        
        public override INode InitBT()
        {
            var Idle = new ActionNode(IdleNode);
        
            var TP = new SequenceNode(
                new ActionNode(TeleportCharge),
                new ActionNode(TeleportAction)
                );

            #region Hide

            var ChangeMask =
                new SelectorNode(
                    true,
                    new ActionNode(ChangeSmile),
                    new ActionNode(ChangeCry),
                    new ActionNode(ChangeAngry)
                );
            
            var Hide = new SelectorNode(
                    true, 
                    new ActionNode(SmokeAttack),
                    ChangeMask
                );  

            #endregion

            #region Attack
            
            #region Smile

            var SmilePattern = new SelectorNode(
                    true,
                    new SequenceNode(
                        new ActionNode(PunchReady),
                            new ActionNode(Punching)
                    ),
                    new SequenceNode(
                        new ActionNode(PunchReady),
                        new ActionNode(FakePunching)
                    )
                    // new ActionNode(ClonePattern)
                );

            var Smile = new SequenceNode(
                    new ActionNode(IsSmile),
                    SmilePattern
                );
            
            #endregion

            #region Cry

            var CryPattern = new SelectorNode(
                    true,
                    new SequenceNode(
                        new ActionNode(CryingShield),
                        new ActionNode(ShieldOffAction)
                    ),
                    new SequenceNode(
                        new ActionNode(ReverseCryingShield),
                        new ActionNode(ShieldOffAction)
                    ),
                    new ActionNode(BreakHat),
                    new ActionNode(NonBreakHat)
                );

            var Cry = new SequenceNode(
                    // new ActionNode(IsCry),
                    CryPattern
                );
            
            #endregion
            
            #region Angry
            
            var AngryPattern = new SelectorNode(
                    true,
                    new ActionNode(HandLazer),
                    new ActionNode(ThrowBoom),
                    new ActionNode(slapAttack)
                );

            var Angry = new SequenceNode(
                    new ActionNode(IsAngry),
                    AngryPattern
                );
            
            #endregion

            var Attack = new SelectorNode(
                    false,
                    // Smile,
                    Cry,
                    Angry
                );
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                // TP,
                ChangeMask  // 임시
                // Hide
                // Attack
            );
        
            var loop = new SequenceNode(
                Idle,
                AttackPattern
            );

            return loop;
        }

        #region Idle

        private INode.NodeState IdleNode()
        {
            if (false == _animationing)
            {
                animator.PlayIdle();
                _animationing = true;
            }

            if(false == animator.IdleTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Idle");
            
            return INode.NodeState.Success;
        }

        #endregion

        #region TP

        private INode.NodeState TeleportCharge()
        {
            if (false == _animationing)
            {
                tpEffect.SendEvent("OnPlay");
                animator.PlayTeleport();
                _animationing = true;
            }

            if (false == animator.TeleportTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"TP");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState TeleportAction()
        {
            TPPositionRPC();
            
            return INode.NodeState.Success;
        }

        #endregion

        #region Hide
        
        private INode.NodeState SmokeAttack()
        {
            if (false == _animationing)
            {
                animator.PlaySmokeAttack();
                // darknessAttackEffect.SendEvent("OnPlay");
                _animationing = true;
            }

            if (false == animator.SmokeTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Smoke Attack");
            
            // 범위 탐색으로 공격 실행
            
            
            return INode.NodeState.Success;
        }

        #region Change Mask

        IEnumerator ChangeMaskCoroutine(MaskType maskType)
        {
            yield return new WaitForSeconds(0.2f);
            ChangeMaskRPC(maskType);
        }
        
        private INode.NodeState ChangeSmile()
        {
            if (false == _animationing)
            {
                animator.PlayMaskChange();
                _animationing = true;
                StartCoroutine(ChangeMaskCoroutine(MaskType.Smile));
            }

            if (false == animator.MaskChangeTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Change Smile");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeCry()
        {
            if (false == _animationing)
            {
                animator.PlayMaskChange();
                _animationing = true;
                StartCoroutine(ChangeMaskCoroutine(MaskType.Cry));
            }

            if (false == animator.MaskChangeTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Change Cry");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeAngry()
        {
            if (false == _animationing)
            {
                animator.PlayMaskChange();
                _animationing = true;
                StartCoroutine(ChangeMaskCoroutine(MaskType.Angry));
            }

            if (false == animator.MaskChangeTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Change Angry");
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #endregion
        
        #region Attack Pattern

        #region Smile

        private Vector3 targetPosition = new Vector3(0, 0, 0);
        private Vector3 fakeTargetPosition = new Vector3(0, 0, 0);
        private int minDistance = int.MaxValue;
        private int type = 0;

        private INode.NodeState PunchReady()
        {
            // 주먹질 애니메이션 실행
            if (false == _animationing)
            {
                // Calculation
                targetPosition = new Vector3(0, 0, 0);
                fakeTargetPosition = new Vector3(0, 0, 0);
                minDistance = int.MaxValue;
            
                // 가까운 대상 계산
                foreach(var player in _players)
                {
                    var distance = (int)(math.distance(player.transform.position, transform.position));
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        targetPosition = player.transform.position;
                        fakeTargetPosition = targetPosition;
                    }
                }
                
                // 공격 범위를 지정해서 구현할까? => 고민 필요
                // if (attackDistance < minDistance)
                //     return INode.NodeState.Failure;
                
                // Animation Play
                animator.PlayPunchReadyAction();
                _animationing = true;

                foreach(var player in _players)
                {
                    if (_players.Length < 2)
                    {
                        fakeTargetPosition = transform.position + new Vector3(0, 10, 0);
                    }
                    else if (player.transform.position != targetPosition)
                    {
                        fakeTargetPosition = (player.transform.position + targetPosition) / 2;
                        break;
                    }        
                }

                type = Random.Range(0, 2);
            }

            if (false == animator.PunchReadyTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Punching Ready");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState Punching()
        {
            // TODO : 먼저 몸을 돌려야 자연스럽지 않을까?
            
            
            if (false == _animationing)
            {
                // dotween으로 주먹 이동 및 충돌 처리
                PunchAttackRPC(type, targetPosition);
                animator.PlayPunchAction();
                _animationing = true;
            }

            if (false == animator.PunchTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Punching");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState FakePunching()
        {
            // TODO : 먼저 몸을 돌려야 자연스럽지 않을까?
            
            
            if (false == _animationing)
            {
                // dotween으로 주먹 절반 이동 및 다른 방향으로 다시 이동 및 충돌 처리
                FakePunchAttackRPC(type, targetPosition, fakeTargetPosition);
                animator.PlayPunchAction();
                _animationing = true;
            }

            if (false == animator.PunchTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Fake Punching");
            
            return INode.NodeState.Success;
        }

        private INode.NodeState ClonePattern()
        {
            // Clone 애니메이션 실행
            // 객체 소환
            DebugManager.Log($"Clone Pattern");
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #region Cry

        
        private INode.NodeState CryingShield()
        {            
            // 애니메이션 실행
            if (false == _animationing)
            {
                animator.PlayShieldAction();
                _animationing = true;
                // shield 파라미터 수정 후 속성값 대입
                _shieldType = 0;
                ShieldOnRPC();
            }

            if (false == animator.ShieldTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Cry Shield");
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ReverseCryingShield()
        {
            // 애니메이션 실행
            if (false == _animationing)
            {
                animator.PlayReverseShieldAction();
                _animationing = true;
                
                // shield 파라미터 수정 후 속성값 대입
                _shieldType = 1;
                ShieldOnRPC();
            }

            if (false == animator.ShieldTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Reverse Cry Shield");
            return INode.NodeState.Success;
        }

        IEnumerator ShieldOnCoroutine()
        {
            float value = 1.1f;
            while (true)
            {
                yield return new WaitForSeconds(0.01f);

                value -= 0.011f;
                bloodShieldMat.SetFloat(Dissolve, value);
                if(value <= 0)
                {
                    // 속성값 대입하는 RPC 실행해야함
                    ShieldAddConditionRPC();
                    yield break;
                }
            }
        }
        
        private INode.NodeState ShieldOffAction()
        {
            if (false == _animationing)
            {
                animator.PlayShieldOffAction();
                _animationing = true;
                
                // shield 파라미터 수정 후 속성값 대입
                ShieldOffRPC();
            }

            if (false == animator.ShieldTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            return INode.NodeState.Success;
        }
        
        IEnumerator ShieldOffCoroutine()
        {
            float value = 0f;
            while (true)
            {
                yield return new WaitForSeconds(0.01f);

                value += 0.011f;
                bloodShieldMat.SetFloat(Dissolve, value);
                if (value >= 1.1f)
                {
                    // 속성값 대입하는 RPC 실행해야함
                    ShieldDelConditionRPC();
                    yield break;
                }
            }
        }
        
        private INode.NodeState BreakHat()
        {
            DebugManager.Log($"Break Hat");
            // 모자 소환 후 모자를 정속성으로 생성
            return INode.NodeState.Success;
        }
        
        private INode.NodeState NonBreakHat()
        {            
            DebugManager.Log($"Non Break Hat");
            // 모자 소환 후 모자를 역속성으로 생성
            return INode.NodeState.Success;
        }

        #endregion
        
        #region Angry

        private INode.NodeState HandLazer()
        {
            DebugManager.Log($"Hand Lazer");
            // 손 내미는 에니메이션 실행
            // running
            
            // VFX실행
            // 데미지 판정
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ThrowBoom()
        {
            DebugManager.Log($"Throw Boom");
            // 폭탄 던지는 애니메이션 실행
            // Running
            
            // 폭탄 소환 ==> 폭탄은 충돌되면 폭발 ( VFX, Damage, Script )
            return INode.NodeState.Success;
        }

        private INode.NodeState slapAttack()
        {
            DebugManager.Log($"Slap Attack");
            // 싸다구 애니메이션 실행
            // 충돌처리
            return INode.NodeState.Success;
        }

        #endregion
        
        private INode.NodeState IsSmile()
        {
            DebugManager.Log($"Is Smile");
            if(_maskType == MaskType.Smile)
                return INode.NodeState.Success;
            
            // Timer 1초 대기

            // 나타나는 애니메이션 있으면 좋을듯
            
            return INode.NodeState.Failure;
        }
        
        private INode.NodeState IsCry()
        {
            DebugManager.Log($"Is Cry");
            if(_maskType == MaskType.Cry)
                return INode.NodeState.Success;
            
            // Timer 1초 대기
            
            // 나타나는 애니메이션 있으면 좋을듯
            
            return INode.NodeState.Failure;
        }
        
        private INode.NodeState IsAngry()
        {
            DebugManager.Log($"Is Angry");
            if(_maskType == MaskType.Angry)
                return INode.NodeState.Success;
            
            // Timer 1초 대기
            
            // 나타나는 애니메이션 있으면 좋을듯
            
            return INode.NodeState.Failure;
        }
        
        #endregion
        
        #endregion

        #region RPC Fuction

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void TPPositionRPC()
        {
            int index = Random.Range(0, 5);

            while (_tpPlaceIndex == index)
                index = Random.Range(0, 5);
            
            DebugManager.Log($"previndex : {_tpPlaceIndex}, curr : {index}");
            
            _tpPlaceIndex = index;
            
            transform.position = tpPlaces[_tpPlaceIndex].position;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ChangeMaskRPC(MaskType Type)
        {
            _masks[(int)(_maskType)].SetActive(false);
            
            _maskType = Type;
            
            _masks[(int)(Type)].SetActive(true);
        }

        #region Punch Attack

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void PunchAttackRPC(int type, Vector3 targetPosition)
        {
            hands[type].transform.DOMove(targetPosition, 2).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
            
            DebugManager.Log($"targetPosition : {targetPosition}");
            
            StartCoroutine(ComeBackPunchCoroutine(2, type));
        }
            
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void FakePunchAttackRPC(int type, Vector3 targetPosition, Vector3 fakeTargetPosition)
        {
            hands[type].transform.DOMove(fakeTargetPosition, 1).SetEase(Ease.OutCirc); // TODO : 공격 속도를 변수처리 해야함

            StartCoroutine(RealTartgetMoveCoroutine(1.0f, type, targetPosition));
            StartCoroutine(ComeBackPunchCoroutine(2, type));
        }

        private IEnumerator RealTartgetMoveCoroutine(float waitTime, int type, Vector3 targetPosition)
        {
            yield return new WaitForSeconds(waitTime);
            RealPunchAttackRPC(type, targetPosition);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RealPunchAttackRPC(int type, Vector3 targetPosition)
        {
            hands[type].transform.DOMove(targetPosition, 1).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }

        private IEnumerator ComeBackPunchCoroutine(float waitTime, int type)
        {
            yield return new WaitForSeconds(waitTime);

            ComeBackPunchRPC(type);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ComeBackPunchRPC(int type)
        {
            float tmp = 0.03f;
            if (type == 0)
                tmp = -0.03f;
            
            hands[type].transform.DOLocalMove(new Vector3(tmp, 0.04f, 0.03f), 1).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }

        #endregion

        #region Shield

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ShieldOnRPC()
        {
            StartCoroutine(ShieldOnCoroutine());
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ShieldOffRPC()
        {
            StartCoroutine(ShieldOffCoroutine());
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ShieldAddConditionRPC()
        {
            if(_shieldType == 0) // Shield
                status.AddCondition(CrowdControl.DamageIgnore);
            else  // Reverse Shield
                status.AddCondition(CrowdControl.DamageReflect);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ShieldDelConditionRPC()
        {
            if(_shieldType == 0)
                status.DelCondition(CrowdControl.DamageIgnore);
            else
                status.DelCondition(CrowdControl.DamageReflect);
        }
        
        #endregion
        
        #endregion
    }
}