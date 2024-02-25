using System;
using BehaviorTree.Base;
using Fusion;
using UnityEngine;

namespace BehaviorTree.Component
{
    public class BTActionExcutorComponent : MonoBehaviour
    {
        private PiggyBankInfo _info;

        private void Awake()
        {
            _info = GetComponent<PiggyBankInfo>();
        }

        bool IsAnimationRunning(string stateName)
        {
            if (_info.animator != null)
            {
                if (_info.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    var normalizedTime = _info.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    return normalizedTime != 0 && normalizedTime < 1f;
                }
            }
            
            return false;
        }

        #region Attack

        /// <summary>
        /// 돼지저금통이 공격을 하는중인지 판단하는 함수
        /// </summary>
        public INode.NodeState CheckAttackAction()
        {
            if (IsAnimationRunning("piggy_attack"))
            {
                return INode.NodeState.Running;
            }

            return INode.NodeState.Success;
        }

        public INode.NodeState StartAttack()
        {
            _info.animator.SetTrigger("tAttack");
            return INode.NodeState.Success;
        }

        // INode.NodeState 
        
        #endregion

        #region Patrol

        public INode.NodeState LookAround()
        {
            
            return INode.NodeState.Success;
        }

        #endregion
        
        #region Walk

        public INode.NodeState WalkAround()
        {
            PiggyBankWalkRPC();
            return INode.NodeState.Success;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void PiggyBankWalkRPC()
        {
            var rb = GetComponent<Rigidbody>();
            
            rb.AddForce(new Vector3(0, 0, -_info.movementSpeed));
        }
        
        #endregion
    }
}