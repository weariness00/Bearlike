using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using UnityEngine;

namespace BehaviorTree.Component
{
    public class PiggyBankInfo : MonoBehaviour
    {
        [Header("Movement")] 
        public float movementSpeed = 1.0f;
        
        #region Property

        public Rigidbody rb;
        public Animator animator = null;

        public BTActionExcutorComponent action;

        public INode root;
        
        #endregion
        
        public enum State
        {
            Idle,
            Patrolling,
            Attacking,
        }

        public State currentState;

        public void UpdateState(State newstate)
        {
            currentState = newstate;
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            root = SettingBT();
            action = GetComponent<BTActionExcutorComponent>();
        }

        INode SettingBT()
        {
            return new SequenceNode(
                new List<INode>()
                {
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(action.CheckAttackAction),
                            new ActionNode(action.StartAttack),
                        }
                    ),
                    new ActionNode(action.WalkAround),
                }
            );
        }
    }
}