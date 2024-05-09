using BehaviorTree.Base;
using Fusion;
using Manager;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class TrumpCardSoldier : MonsterBase
    {
        private BehaviorTreeRunner _behaviorTreeRunner;
        private NetworkObject[] _playerObjects;

        public Transform weaponTransform;
        public CrowdControl crowdControlType;

        [Header("VFX")] 
        public VisualEffect prickVFX; // 찌르는 VFX

        [Header("Animation Clip")]
        public AnimationClip idleClip;
        public AnimationClip walkClip;
        public AnimationClip attackClip;
        
        private bool _isInitAnimation = false;
        [Networked] private TickTimer AniIdleTimer { get; set; }
        [Networked] private TickTimer AniWalkTimer { get; set; }
        [Networked] private TickTimer AniAttackTimer { get; set; }
        
        private Vector3 _randomDir;
        private static readonly int AniMove = Animator.StringToHash("fMove");
        private static readonly int AniAttack = Animator.StringToHash("tAttack");

        public override void Start()
        {
            base.Start();
            
            DebugManager.ToDo("CC 타입 별로 기본 스텟에서 차별을 두기");
            // Spade => 취약
            // Hart => 화상
            // Clover => 독
            // Diamond => Defance?
            if (crowdControlType == CrowdControl.Weak)
            {
                
            }
            else if (crowdControlType == CrowdControl.Burn)
            {
                
            }
            else if (crowdControlType == CrowdControl.Poisoned)
            {
                
            }
            else if (crowdControlType == CrowdControl.DamageIgnore)
            {
                
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            _behaviorTreeRunner.Operator();
        }

        #region Member Function

        private bool CheckDis(float checkDis)
        {
            if (targetTransform == null)
                return false;
            
            var dis = NavMeshDistanceFromTarget(targetTransform.position);  
            return dis < checkDis;   
        }
        
        #endregion

        #region Animation Event Function

        public void AniAttackRayEvent()
        {
            if(!HasStateAuthority)
                return;
            
            LayerMask mask = targetMask | 1 << LayerMask.NameToLayer("Default");
            DebugManager.DrawRay(weaponTransform.position, weaponTransform.up * status.attackRange.Current, Color.magenta, 2f);
            if (Runner.LagCompensation.Raycast(weaponTransform.position, weaponTransform.up,status.attackRange.Current, Runner.LocalPlayer, out var lagHit, mask))
            {
                StatusBase targetStatus;
                if(prickVFX) prickVFX.Play();
                if (lagHit.GameObject.TryGetComponent(out targetStatus) || lagHit.GameObject.transform.root.TryGetComponent(out targetStatus))
                {
                    targetStatus.ApplyDamageRPC(status.CalDamage(), Object.Id, crowdControlType);
                }
            }
        }
        
        #endregion

        #region BT Function

        private INode InitBT()
        {
            var findTarget = new ActionNode(FindTarget);
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move);
            var attack = new Detector(() => CheckDis(status.attackRange.Current), new ActionNode(Attack));

            var offTarget = new SelectorNode(
                true,
                idle,
                move
            );

            var onTarget = new SelectorNode(
                false,
                attack,
                move
            );
            
            var loop = new SequenceNode(
                findTarget,
                new SelectorNode(false,
                    new Detector(() => targetTransform, onTarget),
                    new Detector(() => !targetTransform, offTarget)
                    )
                );
            return loop;
        }

        private INode.NodeState Idle()
        {
            if (!_isInitAnimation)
            {
                _isInitAnimation = true;
                
                networkAnimator.Animator.SetFloat(AniMove, 0);
                
                AniIdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
            }

            if (AniIdleTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            _isInitAnimation = false;
            return INode.NodeState.Success;
        }

        private INode.NodeState Move()
        {
            if (!_isInitAnimation)
            {
                _isInitAnimation = true;
                
                networkAnimator.Animator.SetFloat(AniMove, 1);
                
                _randomDir = Random.onUnitSphere * 2f;
                _randomDir.y = 0;

                AniWalkTimer = TickTimer.CreateFromSeconds(Runner, walkClip.length);
            }

            if (AniWalkTimer.Expired(Runner) == false && navMeshAgent.isOnNavMesh)
            {
                if (targetTransform)
                {
                    navMeshAgent.SetDestination(targetTransform.position);
                }
                else
                {
                    navMeshAgent.SetDestination(transform.position + _randomDir);
                }
                return INode.NodeState.Running;
            }
            
            _isInitAnimation = false;
            networkAnimator.Animator.SetFloat(AniMove, 0);
            return INode.NodeState.Success;
        }

        private INode.NodeState Attack()
        {
            if (!_isInitAnimation)
            {
                _isInitAnimation = true;
                
                networkAnimator.SetTrigger(AniAttack);

                AniAttackTimer = TickTimer.CreateFromSeconds(Runner, attackClip.length + 2f);
            }

            if (AniAttackTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            _isInitAnimation = false;
            return INode.NodeState.Success;
        }
        #endregion
    }
}

