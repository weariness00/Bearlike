﻿using System.Collections.Generic;
using BehaviorTree.Base;
using Fusion;
using Manager;
using Status;
using UI.Status;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class TrumpCardSoldier : MonsterBase
    {
        public Transform weaponTransform;
        public CrowdControl crowdControlType;

        [Header("VFX")] 
        public VisualEffect prickVFX; // 찌르는 VFX

        [Header("Animation Clip")] 
        public TrumpCardSoldierAnimator animatorInfo;
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

        #region Animation Event Function

        public void AniAttackRayEvent()
        {
            if(!HasStateAuthority)
                return;
            
            LayerMask mask = targetMask | 1 << LayerMask.NameToLayer("Default");
            DebugManager.DrawRay(weaponTransform.position, weaponTransform.up * status.attackRange.Current, Color.magenta, 2f);
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
            if (Runner.LagCompensation.RaycastAll(weaponTransform.position, weaponTransform.up,status.attackRange.Current, Runner.LocalPlayer, hits, mask) != 0)
            {
                StatusBase targetStatus;
                if(prickVFX) prickVFX.Play();
                foreach (var hit in hits)
                {
                    if (hit.GameObject.TryGetComponent(out targetStatus) || hit.GameObject.transform.root.TryGetComponent(out targetStatus))
                    {
                        DebugManager.Log($"{hit.GameObject.name}");
                        targetStatus.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, Object.Id, crowdControlType);
                    }
                }
            }
        }
        
        #endregion

        #region BT Function

        public override INode InitBT()
        {
            var findTarget = new ActionNode(FindTarget);
            var idle = new ActionNode(Idle);
            var move = new ActionNode(Move); 
            var attack = new Detector(() => CheckNavMeshDis(status.attackRange.Current), new ActionNode(Attack));

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
                new SelectorNode(
                    false,
                    new Detector(() => aggroController.HasTarget(), onTarget),
                    new Detector(() => !aggroController.HasTarget(), offTarget)
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
            if (navMeshAgent.isOnNavMesh == false || navMeshAgent.isActiveAndEnabled == false)
                return INode.NodeState.Failure;
            
            if (!_isInitAnimation)
            {
                _isInitAnimation = true;
                
                networkAnimator.Animator.SetFloat(AniMove, 1);
                
                _randomDir = Random.onUnitSphere * walkClip.length;
                _randomDir.y = 0;

                AniWalkTimer = TickTimer.CreateFromSeconds(Runner, walkClip.length);
                navMeshAgent.isStopped = !navMeshAgent.isActiveAndEnabled;
                if (aggroController.HasTarget())
                {
                    var target = aggroController.GetTarget();
                    if (IsIncludeLink(target.transform.position))
                        navMeshAgent.stoppingDistance = 0;
                    else
                        navMeshAgent.stoppingDistance = status.attackRange.Current - 0.2f;
                    navMeshAgent.SetDestination(target.transform.position);
                }
                else
                {
                    navMeshAgent.stoppingDistance = 0;
                    navMeshAgent.SetDestination(transform.position + _randomDir);
                }
            }

            if (AniWalkTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            networkAnimator.Animator.SetFloat(AniMove, 0);
            _isInitAnimation = false;
            if(navMeshAgent.isActiveAndEnabled) navMeshAgent.isStopped = true;
            return INode.NodeState.Success;
        }

        private INode.NodeState Jump()
        {
            if (!_isInitAnimation)
            {
                animatorInfo.PlayJump();
            }
            
            if(animatorInfo.JumpTimerExpired == false)
            {
                return INode.NodeState.Running;
            }
            
            return INode.NodeState.Success;
        }

        private INode.NodeState Attack()
        {
            if (navMeshAgent.isOnNavMesh == false || navMeshAgent.isActiveAndEnabled == false)
                return INode.NodeState.Failure;
            
            if (!_isInitAnimation)
            {
                _isInitAnimation = true;
                
                networkAnimator.SetTrigger(AniAttack);

                AniAttackTimer = TickTimer.CreateFromSeconds(Runner, attackClip.length + 1f);
            }

            if (AniAttackTimer.Expired(Runner) == false)
            {
                RotateToTarget();
                return INode.NodeState.Running;
            }

            _isInitAnimation = false;
            return INode.NodeState.Success;
        }
        #endregion
    }
}

