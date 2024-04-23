using System.Linq;
using BehaviorTree.Base;
using Fusion;
using Manager;
using Status;
using UnityEngine;
using UnityEngine.AI;
using Weapon.Bullet;
using Weapon.Gun;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    // 장난감 병정 장총병
    public class ToySoldierGun : MonsterBase
    {
        private BehaviorTreeRunner _behaviorTreeRunner;
        private NetworkObject[] _playerObjects;

        public GunBase gun;

        [Header("Animation Clip")] 
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip longAttackClip;

        private bool _isInitAnimation = false;
        [Networked] private TickTimer AniIdleTimer { get; set; }
        [Networked] private TickTimer AniMoveTimer { get; set; }
        [Networked] private TickTimer AniLongAttackTimer { get; set; }
        private static readonly int AniPropertyMoveSpeed = Animator.StringToHash("f Move Speed");

        #region Unity Event Function

        public override void Start()
        {
            base.Start();
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
            gun.BeforeShootAction += BeforeShoot;
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
                return false;
            
            var dis = StraightDistanceFromTarget(targetTransform.position);
            return dis < checkDis;
        }

        private void BeforeShoot(BulletBase bullet)
        {
            bullet.status.damage.Current = status.damage.Current + gun.status.damage.Current;
            
            bullet.status.attackRange.Max = gun.status.attackRange.Max;
            bullet.status.attackRange.Current = gun.status.attackRange.Current;

            bullet.status.moveSpeed.Current = gun.status.moveSpeed.Current;
        }

        #endregion
        
        #region BT Function

        private INode InitBT()
        {
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move);
            var closeAttack = new Detector(() => CheckDis(1f),new ActionNode(CloseAttack)) ; // 근접 공격
            var longAttack = new Detector(() => CheckDis(status.attackRange.Current),new ActionNode(LongAttack));

            // TargetTransform == null 경우
            var offTarget = new SelectorNode(
                true,
                move,
                idle
            );
            // TargetTransform != null 경우
            var onTarget = new SelectorNode(
                false,
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
                AniIdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
                networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
            }
            if(AniIdleTimer.IsRunning)
            {
                return INode.NodeState.Running;
            }

            networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
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
                AniMoveTimer = TickTimer.CreateFromSeconds(Runner, moveClip.length);
                networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 1f);
                randomDir = Random.onUnitSphere;
                randomDir.y = 0;
            }
            if (AniMoveTimer.Expired(Runner) == false)
            {
                // 타겟 한테 이동
                var path = new NavMeshPath();
                if (targetTransform != null && NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
                {
                    float pathLength = 0.0f;
                    for (int i = 1; i < path.corners.Length; i++)
                        pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                    if (pathLength > status.attackRange.Current)
                    {
                        var dir = path.corners[1] - transform.position;
                        var nextPos = transform.position + Time.deltaTime * dir;
                        transform.LookAt(nextPos);
                        rigidbody.AddForce(ForceMagnitude * status.moveSpeed * transform.forward);
                    }
                    else
                    {
                        networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
                        _isInitAnimation = false;
                        return INode.NodeState.Failure;
                    }
                }
                else
                {
                    var nextPos = transform.position + Time.deltaTime * randomDir;
                    transform.LookAt(nextPos);
                    rigidbody.AddForce(ForceMagnitude * status.moveSpeed * transform.forward);
                }
                
                return INode.NodeState.Running;
            }
            
            networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
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
            // 공격 딜레이가 남아있으면 실패, 총을 쏠 수 있는 상태가 아니면 실패
            if (status.AttackLateTimer.Expired(Runner) == false ||
                gun.FireLateTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Failure;
            }
            
            // 처음 진입 초기화
            if (_isInitAnimation == false)
            {
                rigidbody.velocity = Vector3.zero;
                gun.Shoot(false);
                gun.SetMagazineRPC(StatusValueType.Current, 10);
                _isInitAnimation = true;
                AniLongAttackTimer = TickTimer.CreateFromSeconds(Runner, longAttackClip.length);
                status.StartAttackTimerRPC();
                networkAnimator.SetTrigger("tAttack");
            }
            
            if (AniLongAttackTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            _isInitAnimation = false;
            return INode.NodeState.Success;
        }

        #endregion
    }
}