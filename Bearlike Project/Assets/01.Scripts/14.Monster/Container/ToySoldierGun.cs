using System.Linq;
using BehaviorTree.Base;
using Fusion;
using Manager;
using Status;
using UnityEngine;
using UnityEngine.AI;
using Weapon.Bullet;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    // 장난감 병정 장총병
    public class ToySoldierGun : MonsterBase
    {
        private BehaviorTreeRunner _behaviorTreeRunner;
        private NetworkObject[] _playerObjects;

        [Header("Bullet")]
        public Transform fireTransform; // 총알이 발사할 Transform
        public GameObject bulletPrefab;
        
        [Header("Animation Clip")] 
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip longAttackClip;

        private bool _isInitAnimation = false;
        private StatusValue<float> _aniIdleTime = new StatusValue<float>();
        private StatusValue<float> _aniMoveTime = new StatusValue<float>();
        private StatusValue<float> _aniLongAttackTime = new StatusValue<float>();
        private static readonly int _aniPropertyMoveSpeed = Animator.StringToHash("f Move Speed");

        #region Unity Event Function

        public override void Start()
        {
            base.Start();
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());

            _aniIdleTime.Max = idleClip.length;
            _aniMoveTime.Max = moveClip.length;
            _aniLongAttackTime.Max = longAttackClip.length;
        }

        public override void Spawned()
        {
            base.Spawned();
            _playerObjects = Runner.ActivePlayers.ToArray().Select(player => Runner.GetPlayerObject(player)).ToArray(); // 접속한 플레이어들 저장
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            _behaviorTreeRunner.Operator();
        }

        #endregion

        #region Member Function

        private bool CheckDis(float checkDis)
        {
            if (targetTransform == null)
            {
                return false;
            }
            var dis = StraightDistanceFromTarget(targetTransform.position);
            return dis < checkDis;
        }

        #endregion
        
        #region BT Function

        private INode InitBT()
        {
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move);
            var closeAttack = new Detector(() => CheckDis(1f),new ActionNode(CloseAttack)) ; // 근접 공격
            var longAttack = new Detector(() => CheckDis(5f),new ActionNode(LongAttack));

            // TargetTransform == null 경우
            var offTarget = new SelectorNode(
                true,
                move,
                idle
            );
            // TargetTransform != null 경우
            var onTarget = new SelectorNode(
                false,
                closeAttack,
                longAttack,
                move
            );
            
            var loop = new SequenceNode(
                new ActionNode(FindTarget),
                new SelectorNode(
                    false,
                    new Detector(() => targetTransform == null, offTarget),
                    new Detector(() => targetTransform != null, onTarget)
                )
            );
            return loop;
        }

        private INode.NodeState FindTarget()
        {
            DebugManager.ToDo("어그로 시스템이 없어 가장 가까운 적을 인식하도록 함" +
                              "어그로 시스템을 만들어 인식된 적들중 어그로가 높은 적을 인식하도록 바꾸기");
            
            if (targetTransform == null)
            {
                // 직선 거리상 인식 범위 내에 있는 플레이어 탐색
                var targetPlayers = _playerObjects.Where(player => StraightDistanceFromTarget(player.transform.position) <= status.attackRange.Current + 10f).ToList();
                if (targetPlayers.Count != 0)
                {
                    // 인식범위 내에 있는 아무 플레이어를 Target으로 지정
                    targetTransform = targetPlayers[Random.Range(0, targetPlayers.Count - 1)].transform;
                }
            }
            else
            {
                // Target대상이 인식 범위내에 벗어나면 Target을 풀어주기
                var dis = StraightDistanceFromTarget(targetTransform.position);
                if (dis > status.attackRange.Current +20f)
                {
                    targetTransform = null;
                }
            }

            return INode.NodeState.Success;
        }

        /// <summary>
        /// Idle 애니메이션 한 사이클 만큼 실행
        /// </summary>
        /// <returns></returns>
        private INode.NodeState Idle()
        {
            if (_isInitAnimation == false)
            {
                _isInitAnimation = true;
                _aniIdleTime.Current = 0f;
                networkAnimator.Animator.SetFloat(_aniPropertyMoveSpeed, 0f);
            }
            if(_aniIdleTime.isMax == false)
            {
                _aniIdleTime.Current += Runner.DeltaTime;
                return INode.NodeState.Running;
            }

            networkAnimator.Animator.SetFloat(_aniPropertyMoveSpeed, 0f);
            _isInitAnimation = false;
            return INode.NodeState.Success;
        }

        /// <summary>
        /// Move 애니메이션 한 사이클 만큼 실행
        /// </summary>
        /// <returns></returns>
        private Vector3 randomDir;
        private INode.NodeState Move()
        {
            if (_isInitAnimation == false)
            {
                _isInitAnimation = true;
                _aniMoveTime.Current = 0f;
                networkAnimator.Animator.SetFloat(_aniPropertyMoveSpeed, 1f);
                randomDir = Random.onUnitSphere;
                randomDir.y = 0;
            }
            if (_aniMoveTime.isMax == false)
            {
                // 타겟 한테 이동
                var path = new NavMeshPath();
                if (targetTransform != null && NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
                {
                    if (path.corners.Length > 1)
                    {
                        var dir = path.corners[1] - transform.position;
                        var nextPos = transform.position + Runner.DeltaTime * dir;
                        transform.LookAt(nextPos);
                        transform.position = nextPos;
                    }
                }
                else
                {
                    var nextPos = transform.position + Runner.DeltaTime * randomDir;
                    transform.LookAt(nextPos);
                    transform.position = nextPos;
                }

                
                _aniMoveTime.Current += Runner.DeltaTime;
                return INode.NodeState.Running;
            }
            
            networkAnimator.Animator.SetFloat(_aniPropertyMoveSpeed, 0f);
            _isInitAnimation = false;
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 근접 공격
        /// </summary>
        /// <returns></returns>
        private INode.NodeState CloseAttack()
        {
            return INode.NodeState.Success;
        }

        // 원거리 공격
        private INode.NodeState LongAttack()
        {
            // 공격 딜레이가 남아있으면 실패
            if (status.attackLateTime.isMax == false)
            {
                return INode.NodeState.Failure;
            }
            
            // 처음 진입 초기화
            if (_isInitAnimation == false)
            {
                SpawnBulletRPC();
                _isInitAnimation = true;
                _aniLongAttackTime.Current = 0f;
                status.attackLateTime.Current = 0f;
                networkAnimator.SetTrigger("tAttack");
            }
            
            if (_aniLongAttackTime.isMax == false)
            {
                _aniLongAttackTime.Current += Runner.DeltaTime;
                return INode.NodeState.Running;
            }
            return INode.NodeState.Success;
        }

        #endregion

        #region RPC Function

        /// <summary>
        /// Bullet을 생성해 초기화 해주는 RPC 함수
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void SpawnBulletRPC()
        {
            var bulletBase = bulletPrefab.GetComponent<BulletBase>();
            bulletBase.status.damage.Current += status.damage.Current;
            
            await Runner.SpawnAsync(bulletPrefab, fireTransform.position, fireTransform.rotation);
        }

        #endregion
    }
}