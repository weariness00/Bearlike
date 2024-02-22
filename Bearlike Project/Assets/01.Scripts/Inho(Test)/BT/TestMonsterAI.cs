using System.Collections.Generic;
using BehaviorTree.Base;
using Fusion;
using UnityEngine;

namespace Inho_Test_.BT
{
    [RequireComponent(typeof(Animator))]
    public class TestMonsterAI : NetworkBehaviour
    {
        [Header("Range")] 
        [SerializeField] private float _detectRange = 10.0f;
        [SerializeField] private float _meleeAttackRange = 5.0f;

        [Header("MoveMent")] 
        [SerializeField] private float _movementSpeed = 10.0f;

        private Vector3 _originPos = default;
        private BehaviorTreeRunner _BTRunner = null;
        private Transform _detectedPlayer = null;
        private Animator _animator = null;

        const string _ATTACK_ANIM_STATE_NAME = "Attack";
        const string _ATTACK_ANIM_TIRGGER_NAME = "attack";
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _BTRunner = new BehaviorTreeRunner(SettingBT());
            _originPos = transform.position;
        }

        // 멀티쓰레드 매니저 고류, 멀티쓰레드로 작동하게 하자.
        // monster별로 thread부여 해보자
        private void Update()
        {
            _BTRunner.Operator();
        }

        INode SettingBT()
        {
            return new SelectorNode(
                new List<INode>()
                {
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                        new ActionNode(CheckMeleeAttacking),
                        new ActionNode(CheckEnemyWithinMeleeAttackRange),
                        new ActionNode(DoMeleeAttack),
                        }
                    ),
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            // new ActionNode(CheckDetectEnemy),
                            new ActionNode(MoveToDetectEnemy),
                        }
                    ),
                    new ActionNode(MoveToOriginPosition)
                }
            );
        }

        bool IsAnimationRunning(string stateName)
        {
            if (_animator != null)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    var normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    return normalizedTime != 0 && normalizedTime < 1f;
                }
            }
            
            return false;
        }

        #region Attack Node

        // 공격 중인지 판단하는 함수
        INode.NodeState CheckMeleeAttacking()
        {
            if (IsAnimationRunning(_ATTACK_ANIM_STATE_NAME))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }
        
        // 공격 범위 안에 플레이어가 있는지 판단하는 함수
        INode.NodeState CheckEnemyWithinMeleeAttackRange()
        {
            if (_detectedPlayer != null)
            {
                if (Vector3.SqrMagnitude(_detectedPlayer.position - transform.position) < (_meleeAttackRange * _meleeAttackRange))
                {
                    return INode.NodeState.Success;
                }
            }

            return INode.NodeState.Failure;
        }

        // 공격 시작하는 함수
        INode.NodeState DoMeleeAttack()
        {
            if (_detectedPlayer != null)
            {
                _animator.SetTrigger(_ATTACK_ANIM_TIRGGER_NAME); // 문자열 그대로 넣을까 고민해보자
                return INode.NodeState.Success;
            }

            return INode.NodeState.Failure;
        }
        #endregion

        #region Detect & Move Node
        
        // 적을 발견 했는지 판단하는 함수
        // INode.NodeState CheckDetectEnemy()
        // {
            // var overlapColliders = Physics.OverlapSphere(transform.position, _detectRange, LayerMask.GetMask("Player"));

            // if (overlapColliders != null && overlapColliders.Length > 0)
            // {
            //     _detectedPlayer = overlapColliders[0].transform;
            //
            //     return INode.NodeState.Success;
            // }
            //
            // _detectedPlayer = null;
            //
            // return INode.NodeState.Failure;
        // }

        // 적에게 이동하는 함수
        INode.NodeState MoveToDetectEnemy()
        {
            if (_detectedPlayer != null)
            {
                if (Vector3.SqrMagnitude(_detectedPlayer.position - transform.position) < (_meleeAttackRange * _meleeAttackRange))
                {
                    return INode.NodeState.Success;
                }

                transform.position = Vector3.MoveTowards(transform.position, _detectedPlayer.position, Time.deltaTime * _movementSpeed);

                return INode.NodeState.Running;
            }

            return INode.NodeState.Failure;
        }
        
        #endregion
        
        #region  Move Origin Pos Node
        
        // 원래 자리로 돌아가는 함수
        INode.NodeState MoveToOriginPosition()
        {
            if(Vector3.SqrMagnitude(_originPos - transform.position) < float.Epsilon * float.Epsilon)
            {
                return INode.NodeState.Success;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _originPos, Time.deltaTime * _movementSpeed);
                return INode.NodeState.Running;
            }
        }
        
        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.transform.position, _detectRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, _meleeAttackRange);
        }
    }
}