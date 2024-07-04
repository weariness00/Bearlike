using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using DG.Tweening;
using Fusion;
using Player;
using Status;
using UI.Status;
using Unity.Mathematics;
using UnityEngine;
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
        
        [Header("Hat")]
        [SerializeField] private GameObject[] hat;
        [SerializeField] private Transform[] hatPlaces;

        [Header("AttackObject")] 
        [SerializeField] private GameObject boom;
        [SerializeField] private GameObject lazer;
        [SerializeField] private GameObject breath;
        
        [Header("VFX Properties")]
        [SerializeField] private VisualEffect tpEffect;
        [SerializeField] private VisualEffect darknessAttackEffect;
        [SerializeField] private VisualEffect HandLazerEffect;

        [Header("Effect")] 
        [SerializeField] private Material bloodShieldMat;
        
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        
        [Networked] public NetworkId OwnerId { get; set; }
        
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
        
        public int hatCount;
        private readonly int HATNUM = 4;
        
        #endregion


        #region Unity Event Function

        void Awake()
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
        
        #endregion

        #region Member Function

        public override void Spawned()
        {
            base.Spawned();
            tpEffect.SendEvent("StopPlay");
            darknessAttackEffect.SendEvent("StopPlay");
            HandLazerEffect.gameObject.SetActive(false);

            // TP Position 넣기
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

            
            // Hat Position 넣기
            // rootTrans = transform.root.Find("HatPosition"); // pool에 들어가는 경우
            rootTrans = GameObject.Find("Boss Stage").transform.Find("HatPosition"); // 안들어가는 경우

            if (rootTrans != null)
            {
                hatPlaces = new Transform[rootTrans.childCount];

                for (int i = 0; i < rootTrans.childCount; ++i)
                {
                    DebugManager.Log($"HatPlaces[i] : {hatPlaces[i]}, rootTrans.GetChild(i) : {rootTrans.GetChild(i)}");
                    hatPlaces[i] = rootTrans.GetChild(i);
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

            OwnerId = gameObject.GetComponent<NetworkObject>().Id;
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

            // var ChangeMask =
            //     new SelectorNode(
            //         true,
            //         new ActionNode(ChangeSmile),
            //         new ActionNode(ChangeCry),
            //         new ActionNode(ChangeAngry)
            //     );
            
            var Hide = new SelectorNode(
                    true, 
                    new SequenceNode(
                        new ActionNode(StartSmokeAttack),
                        new ActionNode(SmokingAttack),
                        new ActionNode(EndSmokeAttack)
                    )
                    // new ActionNode(ChangeMaskAction)
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
                    new SequenceNode(
                    new ActionNode(BreakHat),
                        new ActionNode(CheckHatCount)
                    ),
                    new SequenceNode(
                    new ActionNode(BreakReverseHat),
                    new ActionNode(CheckReverseHatCount)
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
                    new ActionNode(HandLazer)
                    // new ActionNode(ThrowBoom),
                    // new ActionNode(slapAttack)
                );

            var Angry = new SequenceNode(
                    // new ActionNode(IsAngry),
                    AngryPattern
                );
            
            #endregion

            var Attack = new SelectorNode(
                    false,
                    // Smile,
                    // Cry,
                    Angry
                );
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                // TP,
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
            
            while (true)
            {
                time += 0.01f;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(playerPosition.x, 0, playerPosition.z)), time);
                yield return new WaitForSeconds(0.01f);
                if(time > 1.0f)
                    yield break;
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
        
        private INode.NodeState StartSmokeAttack()
        {
            if (false == _animationing)
            {
                animator.PlaySmokeStartAttack();
                _animationing = true;
            }

            if (false == animator.SmokeStartTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Start Smoke Attack");
            
            return INode.NodeState.Success;
        }

        private INode.NodeState SmokingAttack()
        {
            if (false == _animationing)
            {
                animator.PlaySmokingAttack();
                darknessAttackEffect.SendEvent("OnPlay");
                _animationing = true;
                
                //==> 그냥 파티클에서 사용한 메쉬를 소환해서 스크립트 하나를 만들자
                Runner.SpawnAsync(breath, transform.position, transform.rotation, null,
                    (runner, o) =>
                    {
                        var h = o.GetComponent<BoxJesterAttackObject>();
                        h.OwnerId = OwnerId;
                        h.damage = 2;
                    });
            }
            // 거리 계산 및 데미지 측정 필요
            // 각도와 거리 계산 필요
            

            if (false == animator.SmokingTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Smoking");
            
            return INode.NodeState.Success;
        }

        private INode.NodeState EndSmokeAttack()
        {
            if (false == _animationing)
            {
                animator.PlaySmokeEndAttack();
                _animationing = true;
            }

            if (false == animator.SmokeEndTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"End Smoke Attack");
            
            return INode.NodeState.Success;
        }

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
                
                DebugManager.Log($"MaskChange : {((MaskType)(tmp)).ToString()}");
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
            // TODO : 코루틴으로 시간 맞춰서 주먹을 움직여야함
            // TODO : 아니면 애니메이션을 두개로 쪼개야함
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
                        targetPosition = player.transform.position + new Vector3(0, 0.75f, 0);
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
            // TODO : 코루틴으로 시간 맞춰서 주먹을 움직여야함
            // TODO : 아니면 애니메이션을 두개로 쪼개야함
            
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
            // TODO : 코루틴으로 시간 맞춰서 주먹을 움직여야함
            // TODO : 아니면 애니메이션을 두개로 쪼개야함
            
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

        #region Hat

        private INode.NodeState BreakHat()
        {
            if (_animationing == false)
            {
                hatCount = HATNUM;
                animator.PlayHatAction();
                _animationing = true;

                List<int> indexList = new List<int>();
                
                for (;indexList.Count < HATNUM;)
                {
                    var index = Random.Range(0, hatPlaces.Length);

                    if (false == indexList.Contains(index))
                        indexList.Add(index);
                }

                foreach (var index in indexList)
                {
                    Runner.SpawnAsync(hat[0].gameObject, hatPlaces[index].position, transform.rotation, null,
                        (runner, o) =>
                        {
                            var h = o.GetComponent<BoxJesterHat>();
                            h.OwnerId = OwnerId;
                            h.hatType = 0;
                        });
                }
                
                // foreach (var hatplace in hatPlaces)
                // {
                //     Runner.SpawnAsync(hat[0].gameObject, hatplace.position, transform.rotation, null,
                //         (runner, o) =>
                //         {
                //             var h = o.GetComponent<BoxJesterHat>();
                //             h.OwnerId = OwnerId;
                //             h.hatType = 0;
                //         });
                // }
            }
            if (false == animator.HatTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Break Hat");
            // 모자 소환 후 모자를 정속성으로 생성
            return INode.NodeState.Success;
        }
        
        private INode.NodeState CheckHatCount()
        {
            DebugManager.Log($"hatCount : {hatCount}");
            if (hatCount > 0)
                status.ApplyHealRPC(10 * hatCount, OwnerId);   // 힐량은 밸런스 측정해서 하자
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState BreakReverseHat()
        {            
            if (_animationing == false)
            {
                hatCount = HATNUM;
                animator.PlayHatAction();
                _animationing = true;
                
                List<int> indexList = new List<int>();
                
                for (;indexList.Count < HATNUM;)
                {
                    var index = Random.Range(0, hatPlaces.Length);

                    if (false == indexList.Contains(index))
                        indexList.Add(index);
                }

                foreach (var index in indexList)
                {
                    Runner.SpawnAsync(hat[1].gameObject, hatPlaces[index].position, transform.rotation, null,
                        (runner, o) =>
                        {
                            var h = o.GetComponent<BoxJesterHat>();
                            h.OwnerId = OwnerId;
                            h.hatType = 1;
                        });
                }
                
                // foreach (var hatplace in hatPlaces)
                // {
                //     Runner.SpawnAsync(hat[1].gameObject, hatplace.position, transform.rotation, null,
                //         (runner, o) =>
                //         {
                //             var h = o.GetComponent<BoxJesterHat>();
                //             h.OwnerId = OwnerId;
                //             h.hatType = 1;
                //         });
                // }
            }
            if (false == animator.HatTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Non Break Hat");
            // 모자 소환 후 모자를 역속성으로 생성
            return INode.NodeState.Success;
        }
        
        private INode.NodeState CheckReverseHatCount()
        {
            DebugManager.Log($"hatCount : {hatCount}");
            
            if (hatCount < HATNUM)
            {
                foreach (var player in _players)
                {
                    player.GetComponent<PlayerStatus>().ApplyDamageRPC(status.damage * hatCount, DamageTextType.Normal, OwnerId);
                }
            }
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #endregion
        
        #region Angry

        private INode.NodeState HandLazer()
        {
            if (false == _animationing)
            {
                animator.PlayHandLazerAction();
                _animationing = true;
                
                Runner.SpawnAsync(lazer, transform.position, transform.rotation, null,
                    (runner, o) =>
                    {
                        var h = o.GetComponent<BoxJesterAttackObject>();
                        h.OwnerId = OwnerId;
                        h.damage = 1;
                    });
            }

            if (false == animator.HandLazerTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Hand Lazer");
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ThrowBoom()
        {
            if (false == _animationing)
            {
                animator.PlayThrowBoomAction();
                _animationing = true;

                StartCoroutine(SpawneBoomCoroutine());
            }

            if (false == animator.ThrowBoomTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Throw Boom");
            
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
                    // Effect 넣어줘야함
                });
        }

        private INode.NodeState slapAttack()
        {
            if (false == _animationing)
            {
                animator.networkAnimator.Animator.enabled = false;
                animator.PlaySlapAction();
                _animationing = true;
                
                SlapStartRPC();
            }

            if (false == animator.SlapTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            animator.networkAnimator.Animator.enabled = true;
            DebugManager.Log($"Slap Attack");
            
            return INode.NodeState.Success;
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
            animator.networkAnimator.Animator.enabled = false;
            hands[type].transform.DOMove(targetPosition, 2).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
            
            DebugManager.Log($"targetPosition : {targetPosition}");
            
            StartCoroutine(ComeBackPunchCoroutine(2, type));
        }
            
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void FakePunchAttackRPC(int type, Vector3 targetPosition, Vector3 fakeTargetPosition)
        {
            animator.networkAnimator.Animator.enabled = false;
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
            yield return new WaitForSeconds(1.0f);
            AnimatorOnRPC();
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ComeBackPunchRPC(int type)
        {
            float tmp = 3f;
            if (type == 0)
                tmp = -3f;
            
            hands[type].transform.DOLocalMove(new Vector3(tmp, 4f, 3f), 1).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void AnimatorOnRPC()
        {
            animator.networkAnimator.Animator.enabled = true;
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
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DestroyHatRPC()
        {
            hatCount -= 1;
        }

        #region Slap

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void SlapStartRPC()
        {
            hands[1].transform.DOLocalMove(new Vector3(5, 3, 2), 1).SetEase(Ease.InCirc);
            hands[1].transform.DOLocalRotate(new Vector3(0, 0, -45f), 1).SetEase(Ease.InCirc);
            StartCoroutine(SlapAttackCoroutine());
        }

        IEnumerator SlapAttackCoroutine()
        {
            yield return new WaitForSeconds(1.5f);

            SlapingRPC();
        }

        private float _radius = 15f;
        private float _duration = 2f;
        private int _segments = 36;
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void SlapingRPC()
        {
            Vector3[] path = new Vector3[_segments];
            float angleStep = 180f / _segments;

            for (int i = 0; i < _segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                path[i] = new Vector3(Mathf.Cos(angle) * _radius, 0.5f, Mathf.Sin(angle) * _radius);
            }

            hands[1].transform.DOLocalPath(path, _duration).SetOptions(true);
            hands[1].transform.DOLocalRotate(new Vector3(0, -180f, 0), _duration);

            StartCoroutine(HandRotationCoroutine());
        }

        IEnumerator HandRotationCoroutine()
        {
            yield return new WaitForSeconds(_duration);
            hands[1].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f);
        }
        
        #endregion
        #endregion
    }
}