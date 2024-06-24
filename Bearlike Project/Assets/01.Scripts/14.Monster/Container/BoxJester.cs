﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using DG.Tweening;
using Fusion;
using Sound;
using Unity.Mathematics;
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

        // public SoundBox soundBox;
        private BehaviorTreeRunner _behaviorTreeRunner;
        private GameObject[] _players;
        private bool animationing = false;
        
        [Header("Animator")]
        [SerializeField] private BoxJesterAnimator animator;
        
        [Header("Teleport Properties")]
        [SerializeField] private Transform[] tpPlaces;

        [Header("HandAttack Properties")] 
        [SerializeField] private GameObject[] hands;
        
        [Header("VFX Properties")]
        [SerializeField] private VisualEffect tpEffect;
        [SerializeField] private VisualEffect darknessAttackEffect;
        
        
        private int _tpPlaceIndex = 0;

        enum MaskType
        {
            Smile,
            Cry,
            Angry
        }
        private MaskType _maskType = MaskType.Smile;
        
        #endregion


        #region Unity Event Function

        void Awake()
        {
            base.Awake();
            
            animator = GetComponentInChildren<BoxJesterAnimator>();
            hands = new GameObject[2];

            // hands[0] = transform.Find("")
            
            // tpEffect.SetFloat("Time", animator.tptClip.length);
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

            var ChangeMask = new SequenceNode(
                    new ActionNode(ChangeAnimation),    
                    new SelectorNode(
                        true,
                        new ActionNode(ChangeSmile),
                        new ActionNode(ChangeCry),
                        new ActionNode(ChangeAngry)
                        )
                    );
            
            var HideSelect = new SelectorNode(
                true, 
                new ActionNode(SmokeAttack),
                ChangeMask
            );
            
            var Hide = new SequenceNode(
                new ActionNode(HideInBox),
                HideSelect,
                new ActionNode(AppearInBox)
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
                    new ActionNode(CryingShield),
                    new ActionNode(ReverseCryingShield),
                    new ActionNode(BreakHat),
                    new ActionNode(NonBreakHat)
                );

            var AnCrygry = new SequenceNode(
                    new ActionNode(IsCry),
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
                    Smile,
                    CryPattern,
                    Angry
                );
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                TP,
                // Hide,
                Attack
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
            if (false == animationing)
            {
                animator.PlayIdle();
                animationing = true;
            }

            if(false == animator.IdleTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            DebugManager.Log($"Idle");
            
            return INode.NodeState.Success;
        }

        #endregion

        #region TP

        private INode.NodeState TeleportCharge()
        {
            if (false == animationing)
            {
                tpEffect.SendEvent("OnPlay");
                animator.PlayTeleport();
                animationing = true;
            }

            if (false == animator.TeleportTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
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

        private INode.NodeState HideInBox()
        {
            if (false == animationing)
            {
                animator.PlayHideInBox();
                animationing = true;
            }

            if (false == animator.HideTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            DebugManager.Log($"Hide On Box");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState SmokeAttack()
        {
            if (false == animationing)
            {
                animator.PlaySmokeAttack();
                // darknessAttackEffect.SendEvent("OnPlay");
                animationing = true;
            }

            if (false == animator.SmokeTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            DebugManager.Log($"Smoke Attack");
            
            // 범위 탐색으로 공격 실행
            
            
            return INode.NodeState.Success;
        }

        #region Change Mask

        private INode.NodeState ChangeAnimation()
        {
            if (false == animationing)
            {
                animator.PlayMaskChange();
                animationing = true;
            }

            if (false == animator.MaskChangeTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            DebugManager.Log($"Mask Change");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeSmile()
        {
            DebugManager.Log($"Change Smile");
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            ChangeMaskRPC(MaskType.Smile);
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeCry()
        {
            DebugManager.Log($"Change Cry");
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            ChangeMaskRPC(MaskType.Cry);
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeAngry()
        {
            DebugManager.Log($"Change Angry");
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            ChangeMaskRPC(MaskType.Angry);
            
            return INode.NodeState.Success;
        }

        #endregion
        
        private INode.NodeState AppearInBox()
        {
            if (false == animationing)
            {
                animator.PlayAppearInBox();
                animationing = true;
            }

            if (false == animator.AppearTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            DebugManager.Log($"Appear In Box");
            
            return INode.NodeState.Success;
        }
        
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
            if (false == animationing)
            {
                animator.PlayPunchReadyAction();
                animationing = true;
            }

            if (false == animator.PunchReadyTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            
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
                }
            }

            foreach(var player in _players)
            {
                if (player.transform.position != targetPosition)
                {
                    fakeTargetPosition = player.transform.position;
                    break;
                }           
            }
            
            // 공격 범위를 지정해서 구현할까? => 고민 필요
            // if (attackDistance < minDistance)
            //     return INode.NodeState.Failure;

            type = Random.Range(0, 2);
            
            DebugManager.Log($"Punching Ready");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState Punching()
        {
            // TODO : 먼저 몸을 돌려야 자연스럽지 않을까?
            
            // dotween으로 주먹 이동 및 충돌 처리
            PunchAttackRPC(type, targetPosition);
            
            DebugManager.Log($"Punching");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState FakePunching()
        {
            // TODO : 먼저 몸을 돌려야 자연스럽지 않을까?
            // dotween으로 주먹 절반 이동 및 다른 방향으로 다시 이동 및 충돌 처리
            FakePunchAttackRPC(type, targetPosition, fakeTargetPosition);
            
            
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
            DebugManager.Log($"Cry Shield");
            // 애니메이션 실행
            // shield 파라미터 수정 후 속성값 대입
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ReverseCryingShield()
        {
            DebugManager.Log($"Reverse Cry Shield");
            // 애니메이션 실행
            // shield 파라미터 수정 후 속성값 대입
            return INode.NodeState.Success;
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
            _maskType = Type;
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void PunchAttackRPC(int type, Vector3 targetPosition)
        {
            hands[type].transform.DOMove(targetPosition, 2).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }
            
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void FakePunchAttackRPC(int type, Vector3 targetPosition, Vector3 fakeTargetPosition)
        {
            hands[type].transform.DOMove(fakeTargetPosition, 1).SetEase(Ease.OutCirc); // TODO : 공격 속도를 변수처리 해야함

            StartCoroutine(RealTartgetMoveCoroutine(1.0f, type, targetPosition));
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
        
        
        #endregion
    }
}