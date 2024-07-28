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
        [SerializeField] private Transform[] cloneTPPlaces;

        [Header("HandAttack Properties")] 
        [SerializeField] private GameObject[] hands;
        [SerializeField] private GameObject hand;
        [SerializeField] private float punchTime;
        
        [Header("Hat")]
        [SerializeField] private GameObject[] hat;
        [SerializeField] private Transform[] hatPlaces;

        [Header("AttackObject")] 
        [SerializeField] private GameObject boom;
        [SerializeField] private GameObject lazer;
        [SerializeField] private GameObject breath;
        [SerializeField] private GameObject cloneObject;
        
        // [Header("VFX Properties")]
        // [SerializeField] private VisualEffect tpEffect;
        // [SerializeField] private VisualEffect darknessAttackEffect;
        // [SerializeField] private VisualEffect handLazerEffect;

        [Header("Effect")] 
        [SerializeField] private Material bloodShieldMat;
        
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        
        [Networked] public NetworkId OwnerId { get; set; }
        
        // public SoundBox soundBox;
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
        
        public int hatCount;
        private readonly int HATNUM = 4;
        
        #endregion


        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();
            
            animator = GetComponentInChildren<BoxJesterAnimator>();
            
            // tpEffect.SetFloat("Time", animator.tpClip.length);

            var bloodShieldRenderer = transform.Find("ShieldEffect").GetComponent<Renderer>();
            bloodShieldMat = bloodShieldRenderer.material;
            
            // Mask 대입
            var boxJester = transform.Find("Clown");
            
            _masks = new GameObject[3];
            _masks[0] = boxJester.Find("Smile_Face").gameObject;
            _masks[1] = boxJester.Find("Sad_Face").gameObject;
            _masks[2] = boxJester.Find("Angry_Face").gameObject;

            DieAction += () => animator.PlayDieAction();
            DieAction += () => Destroy(gameObject, 3);

            _handModel = transform.Find("Clown").Find("Hand").gameObject;
        }
        
        #endregion

        #region Member Function

        public override void Spawned()
        {
            base.Spawned();

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
            
            // Clone TP
            rootTrans = GameObject.Find("Boss Stage").transform.Find("CloneTPPosition"); // 안들어가는 경우

            if (rootTrans != null)
            {
                cloneTPPlaces = new Transform[rootTrans.childCount];

                for (int i = 0; i < rootTrans.childCount; ++i)
                {
                    DebugManager.Log($"tpPlaces[i] : {tpPlaces[i]}, rootTrans.GetChild(i) : {rootTrans.GetChild(i)}");
                    cloneTPPlaces[i] = rootTrans.GetChild(i);
                }
            }
            else
            {
                DebugManager.LogError($"BoxJester의 CloneTPPosition이 NULL입니다.");
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
        
        private void OnCollisionEnter(Collision other)
        {
            StatusBase otherStatus = null;

            if (status.IsDie == false)
            {
                if (true == other.gameObject.CompareTag("Player"))
                {
                    if (other.gameObject.TryGetComponent(out otherStatus) ||
                        other.transform.root.gameObject.TryGetComponent(out otherStatus))
                    {
                        status.AddAdditionalStatus(otherStatus);
                        // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                        //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                        otherStatus.ApplyDamageRPC(5, DamageTextType.Normal, OwnerId);
                        status.RemoveAdditionalStatus(otherStatus);
                    }
                }
            }
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
            
            var Hide = new SelectorNode(
                    true, 
                    new SequenceNode(
                        new ActionNode(StartSmokeAttack),
                        new ActionNode(SmokingAttack),
                        new ActionNode(EndSmokeAttack)
                    ),
                    new ActionNode(ChangeMaskAction)
                );  

            #endregion

            #region Attack
            
            #region Smile

            var SmilePattern = new SelectorNode(
                    true,
                    // new SequenceNode(
                    //     new ActionNode(PunchReady),
                    //         new ActionNode(Punching)
                    // ),
                    // new SequenceNode(
                    //     new ActionNode(PunchReady),
                    //     new ActionNode(FakePunching)
                    // ),
                    new ActionNode(ClonePattern)
                );

            var Smile = new SequenceNode(
                    new ActionNode(IsSmile),
                    SmilePattern
                );
            
            #endregion

            #region Cry

            var CryPattern = new SelectorNode(
                    true,
                    // new SequenceNode(
                    //     new ActionNode(CryingShield),
                    //     new ActionNode(ShieldOffAction)
                    // ),
                    new SequenceNode(
                        new ActionNode(ReverseCryingShield),
                        new ActionNode(ShieldOffAction)
                    )
                    // new SequenceNode(
                    // new ActionNode(BreakHat),
                    //     new ActionNode(CheckHatCount)
                    // ),
                    // new SequenceNode(
                    // new ActionNode(BreakReverseHat),
                    // new ActionNode(CheckReverseHatCount)
                    // )
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
                    Smile
                    // Cry
                    // Angry
                );
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                // TP,
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

            DebugManager.Log($"playerPosition : {playerPosition}");
            
            float time = 0.0f;
            
            while (time < 1.0f)
            {
                time += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(playerPosition.x, 0, playerPosition.z)), time);
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
            DebugManager.Log($"Idle");
            
            return INode.NodeState.Success;
        }

        #endregion

        #region TP

        private INode.NodeState TeleportCharge()
        {
            if (false == _animationing)
            {
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
                _animationing = true;
                
                Runner.SpawnAsync(breath, transform.position, transform.rotation, null,
                    (runner, o) =>
                    {
                        var h = o.GetComponent<BoxJesterAttackObject>();
                        h.OwnerId = OwnerId;
                        h.damage = 2;
                    });
            }

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
                StartCoroutine(RotationCoroutine());
                
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
                
                GameObject playerObject = _players[0];
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
                        playerObject = player;
                        minDistance = distance;
                        targetPosition = player.transform.position + new Vector3(0, 2f, 0);
                        fakeTargetPosition = targetPosition;
                    }
                }
                
                // 공격 범위를 지정해서 구현할까? => 고민 필요
                // if (attackDistance < minDistance)
                //     return INode.NodeState.Failure;
                
                foreach(var player in _players)
                {
                    if (_players.Length < 2)
                    {
                        fakeTargetPosition = transform.position + new Vector3(0, 20, 0);
                    }
                    else if (playerObject != player)
                    {
                        fakeTargetPosition = (player.transform.position + targetPosition) / 2;
                        break;
                    }
                }

                type = Random.Range(0, 2);
                
                // Animation Play
                animator.PlayPunchReadyAction();
                _animationing = true;
            }

            if (false == animator.PunchReadyTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            
            DebugManager.Log($"Punching Ready");
            
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

                        // h.position = _handModel.transform.position;
                        // h.rotation = transform.rotation;
                        
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
            DebugManager.Log($"Punching");
            
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

                        // h.position = _handModel.transform.position;
                        // h.rotation = transform.rotation;
                        
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
            DebugManager.Log($"Fake Punching");
            
            return INode.NodeState.Success;
        }

        private INode.NodeState ClonePattern()
        {
            if (false == _animationing)
            {
                animator.PlayCloneAction();
                _animationing = true;
            }

            if (false == animator.CloneTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            DebugManager.Log($"Clone Pattern");

            int randomPlace = Random.Range(0, cloneTPPlaces.Length);
            
            Runner.SpawnAsync(cloneObject, cloneTPPlaces[randomPlace].position, transform.rotation, null,
                (runner, o) =>
                {
                    var h = o.GetComponent<BoxJesterClone>();

                    h.OwnerId = OwnerId;
                });
            
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
                StartCoroutine(RotationCoroutine());
                
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
                    // try
                    // {
                        Runner.SpawnAsync(hat[0].gameObject, hatPlaces[index].position, transform.rotation, null,
                            (runner, o) =>
                            {
                                var h = o.GetComponent<BoxJesterHat>();
                                h.OwnerId = OwnerId;
                            });
                        DebugManager.Log("BlueHat Spawn");
                    // }
                    // catch (Exception ex)
                    // {
                    //     DebugManager.LogError($"Hat Spawn Failed : {ex}");
                    // }
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
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState CheckHatCount()
        {
            DebugManager.Log($"hatCount : {hatCount}");
            if (hatCount > 0)
                status.ApplyHealRPC(100 * hatCount, OwnerId);   // 힐량은 밸런스 측정해서 하자
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState BreakReverseHat()
        {            
            if (_animationing == false)
            {
                StartCoroutine(RotationCoroutine());
                
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
                        });
                    DebugManager.Log("RedHat Spawn");
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
            
            return INode.NodeState.Success;
        }
        
        private INode.NodeState CheckReverseHatCount()
        {
            DebugManager.Log($"hatCount : {hatCount}");
            
            if (hatCount > 0)
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
                
                //TODO : 준비동작에서는 데미지가 안 들어가도록 수정해야함
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
                StartCoroutine(RotationCoroutine());
                
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
                });
        }

        private INode.NodeState slapAttack()
        {
            if (false == _animationing)
            {
                animator.enabled = false;
                animator.PlaySlapAction();
                _animationing = true;
                
                SlapStartRPC();
            }

            if (false == animator.SlapTimerExpired)
                return INode.NodeState.Running;

            _animationing = false;
            animator.enabled = true;
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