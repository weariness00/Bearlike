using BehaviorTree.Base;
using Fusion;
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

            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;
            if (NavMesh.SamplePosition(transform.position, out var hit, 10.0f, NavMesh.AllAreas))
                transform.position = hit.position; // NavMesh 위치로 이동
            navMeshAgent.enabled = true;
            
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
            gun.BeforeShootAction += BeforeShoot;
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            _behaviorTreeRunner.Operator();
        }

        #endregion

        #region Member Function

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
            var findTarget = new ActionNode(FindTarget);
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move);
            var closeAttack = new Detector(() => CheckStraightDis(1f),new ActionNode(CloseAttack)) ; // 근접 공격
            var longAttack = new Detector(() => CheckStraightDis(status.attackRange.Current),new ActionNode(LongAttack));

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
                findTarget,
                new SelectorNode(
                    false,
                    new Detector(() => targetPlayer == null, offTarget),
                    new Detector(() => targetPlayer != null, onTarget)
                )
            );
            return loop;
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

            if (AniIdleTimer.Expired(Runner) == false)
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
            if (CheckStraightDis(status.attackRange.Current - 1.0f))
            {
                networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
                _isInitAnimation = false;
                navMeshAgent.isStopped = true;
                return INode.NodeState.Success;
            }
            
            if (_isInitAnimation == false)
            {
                _isInitAnimation = true;
                navMeshAgent.isStopped = false;
                AniMoveTimer = TickTimer.CreateFromSeconds(Runner, moveClip.length);
                networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 1f);
                randomDir = Random.onUnitSphere * 2f;
                randomDir.y = 0;
                navMeshAgent.speed = status.moveSpeed.Current;
            }
            
            if (AniMoveTimer.Expired(Runner) == false && navMeshAgent.isOnNavMesh)
            {
                // 타겟 한테 이동
                if (targetPlayer)
                {
                    navMeshAgent.stoppingDistance = status.attackRange.Current - 1.0f;
                    navMeshAgent.SetDestination(targetPlayer.transform.position);
                }
                else
                {
                    var nextPos = transform.position + randomDir;
                    navMeshAgent.stoppingDistance = 0;
                    navMeshAgent.SetDestination(nextPos);
                }
                return INode.NodeState.Running;
            }
            
            networkAnimator.Animator.SetFloat(AniPropertyMoveSpeed, 0f);
            _isInitAnimation = false;
            navMeshAgent.isStopped = true;
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
            if (status.AttackLateTimer.Expired(Runner) == false)
            {
                Vector3 dir = (targetPlayer.transform.position - transform.position).normalized;
                dir.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Runner.DeltaTime);
                return INode.NodeState.Failure;
            }
            
            // 처음 진입 초기화
            if (_isInitAnimation == false)
            {
                gun.FireBullet(false);
                gun.SetMagazineRPC(StatusValueType.Current, 10);
                _isInitAnimation = true;
                AniLongAttackTimer = TickTimer.CreateFromSeconds(Runner, longAttackClip.length);
                networkAnimator.SetTrigger("tAttack");

                gun.FireLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
            }
            
            if (AniLongAttackTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            _isInitAnimation = false;
            status.StartAttackTimerRPC();
            return INode.NodeState.Success;
        }

        #endregion
    }
}