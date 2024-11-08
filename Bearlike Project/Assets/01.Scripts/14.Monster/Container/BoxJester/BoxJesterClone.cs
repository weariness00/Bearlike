﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using DG.Tweening;
using Fusion;
using Manager;
using Status;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    public class BoxJesterClone : MonsterBase
    {
        #region Properties

        [Header("Animator")]
        [SerializeField] private BoxJesterAnimator animator;
        
        [Header("Teleport Properties")]
        [SerializeField] private Transform[] tpPlaces;

        [Header("HandAttack Properties")] 
        [SerializeField] private GameObject[] hands;
        [SerializeField] private GameObject hand;
        [SerializeField] private float punchTime;
        
        [Header("AttackObject")] 
        [SerializeField] private GameObject boom;
        
        [Header("VFX Properties")]
        [SerializeField] private VisualEffect tpEffect;
        
        [Header("Effect")] 
        [SerializeField] private Material bloodShieldMat;
        
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        
        [Networked] public NetworkId OwnerId { get; set; }
        [Networked] public NetworkId MyId { get; set; }
        
        private BehaviorTreeRunner _behaviorTreeRunner;
        private GameObject[] _players;
        private GameObject[] _masks;
        private GameObject _handModel;
        
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

        private readonly float _punchAttackDistance = 50.0f;
        private BoxJester boxJester;

        #endregion
        
        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();
            
            animator = GetComponentInChildren<BoxJesterAnimator>();
            
            tpEffect.SetFloat("Time", animator.tpClip.length);

            var bloodShieldRenderer = transform.Find("ShieldEffect").GetComponent<Renderer>();
            bloodShieldMat = bloodShieldRenderer.material;
            
            // Mask 대입
            var boxJester = transform.Find("Clown");
            
            _masks = new GameObject[3];
            _masks[0] = boxJester.Find("Smile_Face").gameObject;
            _masks[1] = boxJester.Find("Sad_Face").gameObject;
            _masks[2] = boxJester.Find("Angry_Face").gameObject;
        }

        private void OnDestroy()
        {
            boxJester.isCloneSpawned = false;
        }
        #endregion

        #region Member Function

        public override void Spawned()
        {
            base.Spawned();
            tpEffect.SendEvent("StopPlay");

            Transform rootTrans = GameObject.Find("Boss Stage").transform.Find("CloneTPPosition"); // 안들어가는 경우

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
            
            
            var ownerObj = Runner.FindObject(OwnerId);
            boxJester = ownerObj.gameObject.GetComponent<BoxJester>();
            
            DieAction += () =>
            {
                boxJester.isCloneSpawned = false;
                HandActiveRPC(true);
                animator.PlayDieAction();
                Destroy(gameObject, 3);
            };
            
            MyId = gameObject.GetComponent<NetworkObject>().Id;
            
            _handModel = transform.Find("Clown").Find("Hand").gameObject;
            
            Destroy(gameObject, 20.0f);
        }
        
        #endregion
        
        
        public override INode InitBT()
        {
            var Idle = new ActionNode(IdleNode);

            #region Hide

            var Hide = new ActionNode(ChangeMaskAction);

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
                    )
                );

            var Cry = new SequenceNode(
                    new ActionNode(IsCry),
                    CryPattern
                );
            
            #endregion
            
            #region Angry
            
            var AngryPattern = new SelectorNode(
                    true,
                    new ActionNode(ThrowBoom)
                );

            var Angry = new SequenceNode(
                    new ActionNode(IsAngry),
                    AngryPattern
                );
            
            #endregion

            var Attack = new SelectorNode(
                    false,
                    Smile,
                    Cry,
                    Angry
                );
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                Hide,
                Attack
            );
        
            var loop = new SequenceNode(
                Idle,
                AttackPattern
            );

            return loop;
        }
        
        #region Idle

        IEnumerator RotationCoroutine()
        {
            var mindistance = float.MaxValue;
            var playerPosition = new Vector3(0, 0, 0);
            foreach (var player in _players)
            {
                var dis = math.distance(transform.position, player.transform.position);
                if (dis < mindistance)
                {
                    mindistance = dis;
                    playerPosition = player.transform.position;
                }
            }

            float time = 0.0f;
            
            while (time < 1.0f)
            {
                time += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(playerPosition.x, 0, playerPosition.z) - new Vector3(transform.position.x, 0, transform.position.z)), time);
                yield return null;
            }
        }
        
        private INode.NodeState IdleNode()
        {
            if (false == _animationing)
            {
                animator.PlayIdle();
                StartCoroutine(RotationCoroutine());
                _animationing = true;
            }

            if(false == animator.IdleTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Clone Idle");
            
            return INode.NodeState.Success;
        }

        #endregion

        #region Hide
        
        #region Change Mask

        private INode.NodeState ChangeMaskAction()
        {
            if (false == _animationing)
            {
                int tmp = Random.Range(0, 3);

                while (tmp == (int)(_maskType))
                {
                    tmp = Random.Range(0, 3);
                }
                
                animator.PlayMaskChange();
                _animationing = true;
                StartCoroutine(ChangeMaskCoroutine((MaskType)(tmp)));
                
                DebugManager.Log($"Clone MaskChange : {((MaskType)(tmp)).ToString()}");
            }

            if (false == animator.MaskChangeTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            return INode.NodeState.Success;
        }

        IEnumerator ChangeMaskCoroutine(MaskType maskType)
        {
            yield return new WaitForSeconds(0.2f);
            ChangeMaskRPC(maskType);
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
            if (false == _animationing)
            {
                StartCoroutine(RotationCoroutine());
                
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
                        targetPosition = player.transform.position + new Vector3(0, 2f, 0);
                        fakeTargetPosition = targetPosition;
                    }
                }
                
                if (_punchAttackDistance < minDistance)
                    return INode.NodeState.Failure;
                
                foreach(var player in _players)
                {
                    if (_players.Length < 2)
                    {
                        fakeTargetPosition = transform.position + new Vector3(0, 20, 0);
                    }
                    else if (player.transform.position != targetPosition - new Vector3(0, 2f, 0))
                    {
                        fakeTargetPosition = (player.transform.position + targetPosition) / 2;
                        break;
                    }
                }
                
                if (_punchAttackDistance < math.distance(transform.position, fakeTargetPosition))
                    return INode.NodeState.Failure;

                type = Random.Range(0, 2);
                
                // Animation Play
                animator.PlayPunchReadyAction();
                _animationing = true;
            }

            if (false == animator.PunchReadyTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Clone Punching Ready");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState Punching()
        {
            if (false == _animationing)
            {
                HandActiveRPC(false);
                
                Runner.SpawnAsync(hand, _handModel.transform.position, transform.rotation, null,
                    (runner, o) =>
                    {
                        var h = o.GetComponent<BoxJesterAttackHand>();

                        h.OwnerId = MyId;
                        h.targetPosition = targetPosition;
                        h.handType = type;
                        h.isFake = false;
                        h.time = punchTime;
                    });
                
                animator.PlayPunchAction();
                _animationing = true;
            }

            if (false == animator.PunchTimerExpired)
                return INode.NodeState.Running;
            
            HandActiveRPC(true);
            
            _animationing = false;
            DebugManager.Log($"Clone Punching");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState FakePunching()
        {
            if (false == _animationing)
            {
                HandActiveRPC(false);
                
                Runner.SpawnAsync(hand, _handModel.transform.position, transform.rotation, null,
                    (runner, o) =>
                    {
                        var h = o.GetComponent<BoxJesterAttackHand>();

                        h.OwnerId = MyId;
                        h.targetPosition = targetPosition;
                        h.fakeTargetPosition = fakeTargetPosition;
                        h.handType = type;
                        h.isFake = true;
                        h.time = punchTime;
                    });
                
                animator.PlayPunchAction();
                _animationing = true;
            }

            if (false == animator.PunchTimerExpired)
                return INode.NodeState.Running;

            HandActiveRPC(true);
            
            _animationing = false;
            DebugManager.Log($"Clone Fake Punching");
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #region Cry

        #region Shield

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

        #endregion
        
        #endregion
        
        #region Angry
        
        private INode.NodeState ThrowBoom()
        {
            if (false == _animationing)
            {
                StartCoroutine(RotationCoroutine());
                animator.PlayThrowBoomAction();
                _animationing = true;

                StartCoroutine(SpawneBoomCoroutine());
            }

            if (false == animator.ThrowBoomTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Clone Throw Boom");
            
            return INode.NodeState.Success;
        }

        IEnumerator SpawneBoomCoroutine()
        {
            yield return new WaitForSeconds(1.1f);

            var pos = (hands[0].transform.position + hands[1].transform.position) / 2;
            pos -= transform.forward * 2;
            pos += transform.up * 3;
            
            Runner.SpawnAsync(boom, pos, transform.rotation, null,
                (runner, o) =>
                {
                    var h = o.GetComponent<BoxJesterBoom>();
                    h.OwnerId = OwnerId;
                    h.dir = transform.forward;
                });
        }

        #endregion

        #region Cal Mask

        private INode.NodeState IsSmile()
        {
            DebugManager.Log($"Is Smile");
            if(_maskType == MaskType.Smile)
                return INode.NodeState.Success;
            
            return INode.NodeState.Failure;
        }
        
        private INode.NodeState IsCry()
        {
            DebugManager.Log($"Is Cry");
            if(_maskType == MaskType.Cry)
                return INode.NodeState.Success;
            
            return INode.NodeState.Failure;
        }
        
        private INode.NodeState IsAngry()
        {
            DebugManager.Log($"Is Angry");
            if(_maskType == MaskType.Angry)
                return INode.NodeState.Success;
            
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
        private void HandActiveRPC(bool value)
        {
            _handModel.SetActive(value);
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