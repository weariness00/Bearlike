using BehaviorTree.Base;
using Fusion;
using UnityEngine;

namespace Monster.Container
{
    public class ToySoldierSword : MonsterBase
    {
        public Transform weaponTransform;

        // 찌르기 공격 딜레이
        [HideInInspector] public float stabbingAttackDamageMultiple = 1f; // 찌르기 공격의 대미지 배율
        private float stabbingAttackLate;
        private TickTimer stabbingAttackTimer;
        [HideInInspector] public float stabbingDistance; // 찌르기를 할때 나아가는 거리
        
        // 애니메이터
        private ToySoldierSwordAnimator animator;
        private bool isInitAnimation = false;

        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<ToySoldierSwordAnimator>();
        }

        public override void Start()
        {
            base.Start();
            var stateData = GetStatusData(id);
            stabbingAttackDamageMultiple = stateData.GetFloat("Stabbing Attack Damage Multiple");
            stabbingAttackLate = stateData.GetFloat("Stabbing Attack Late");
            stabbingDistance = stateData.GetFloat("Stabbing Distance");
        }

        public override void Spawned()
        {
            base.Spawned();
            stabbingAttackTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        #endregion

        #region Member Function

        // 몸이 물체를 관통하는 상태로 만듬
        // 기본적인 건물과 공격 물체들은 관통하지 못함
        // 시체, 플레이어, 몬스터 등만 관통
        private void BodyPenetrate()
        {
            
        }

        #endregion
        
        #region BT Function

        public override INode InitBT()
        {
            var findTarget = new ActionNode(FindTarget);
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move);
            var closeAttack = new Detector(() => CheckStraightDis(1f),new ActionNode(DefaultAttack)) ; // 근접 공격
            var longAttack = new Detector(() => stabbingAttackTimer.Expired(Runner) && CheckStraightDis(status.attackRange.Current), new ActionNode(StabbingAttack));

            // TargetTransform == null 경우
            var offTarget = new SelectorNode(
                true,
                move,
                idle
            );
            // TargetTransform != null 경우
            var onTarget = new SelectorNode(
                false,
                new SelectorNode(
                    true,
                    closeAttack,
                    longAttack
                    ),
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
            if (isInitAnimation == false)
            {
                isInitAnimation = true;
                animator.PlayIdle();
            }

            if (animator.IdleTimerExpired == false)
            {
                return INode.NodeState.Running;
            }

            isInitAnimation = false;
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
                isInitAnimation = false;
                if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
                return INode.NodeState.Success;
            }
            
            if (isInitAnimation == false)
            {
                isInitAnimation = true;
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
            isInitAnimation = false;
            if(navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            return INode.NodeState.Success;
        }

        /// <summary>
        /// 근접 공격
        /// </summary>
        /// <returns></returns>
        private INode.NodeState DefaultAttack()
        {
            // 처음 진입 초기화
            if (isInitAnimation == false)
            {
                isInitAnimation = true;
                animator.AttackSpeed = status.attackSpeed.Current;
                animator.PlayDefaultAttack();
            }
            
            if (animator.DefaultAttackTimerExpired == false)
            {
                return INode.NodeState.Running;
            }

            isInitAnimation = false;
            return INode.NodeState.Success;
        }

        // 긴 거리 공격
        private INode.NodeState StabbingAttack()
        {
            // 처음 진입 초기화
            if (isInitAnimation == false)
            {
                isInitAnimation = true;
                animator.AttackSpeed = status.attackSpeed.Current;
                animator.PlayStabbingAttack();
                
                DisableNavMeshAgent(false, true);
                rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            
            if (animator.StabbingAttackTimerExpired == false)
            {
                return INode.NodeState.Running;
            }

            EnableNavMeshAgent();
            rigidbody.constraints = RigidbodyConstraints.None;
            stabbingAttackTimer = TickTimer.CreateFromSeconds(Runner, stabbingAttackLate);
            isInitAnimation = false;
            return INode.NodeState.Success;
        }
        
        #endregion
    }
}