using BehaviorTree.Base;
using Fusion;
using Status;
using UnityEngine;
using Weapon.Gun;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    // 장난감 병정 장총병
    public class ToySoldierGun : MonsterBase
    {
        [SerializeField] private ToySoldierGunAnimator animator;
        public GunBase gun;

        private bool _isInitAnimation = false;

        public override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<ToySoldierGunAnimator>();
        }

        #region BT Function

        public override INode InitBT()
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
                    new Detector(() => !aggroController.HasTarget(), offTarget),
                    new Detector(() =>  aggroController.HasTarget(), onTarget)
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
                animator.PlayIdle();
            }

            if (animator.IdleTimerExpired == false)
            {
                return INode.NodeState.Running;
            }

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
                animator.MoveSpeed = 0f;
                _isInitAnimation = false;
                if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
                return INode.NodeState.Success;
            }
            
            if (_isInitAnimation == false)
            {
                _isInitAnimation = true;
                if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;
                animator.PlayMove();
                randomDir = Random.onUnitSphere * 2f;
                randomDir.y = 0;
                navMeshAgent.speed = status.moveSpeed.Current;
            }
            
            if (animator.MoveTimerExpired == false && navMeshAgent.isOnNavMesh)
            {
                // 타겟 한테 이동
                if ( aggroController.HasTarget())
                {
                    navMeshAgent.stoppingDistance = status.attackRange.Current - 1.0f;
                    navMeshAgent.SetDestination( aggroController.GetTarget().transform.position);
                }
                else
                {
                    var nextPos = transform.position + randomDir;
                    navMeshAgent.stoppingDistance = 0;
                    navMeshAgent.SetDestination(nextPos);
                }
                return INode.NodeState.Running;
            }
            
            animator.MoveSpeed = 0f;
            _isInitAnimation = false;
            if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
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
                Vector3 dir = (aggroController.GetTarget().transform.position - transform.position).normalized;
                dir.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Runner.DeltaTime);
                return INode.NodeState.Failure;
            }
            
            // 처음 진입 초기화
            if (_isInitAnimation == false)
            {
                _isInitAnimation = true;
                animator.PlayLongAttack();
            }
            
            if (animator.LongAttackTimerExpired == false)
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