using BehaviorTree.Base;
using UnityEngine;

namespace BehaviorTree.Component.PiggyBank
{
    public class PiggyBankInfo : MonoBehaviour
    {
        [Header("Movement")] 
        public float movementSpeed = 1.0f;
        
        #region Property

        public Rigidbody rb;
        public INode root;
        
        #endregion
        
        public enum StateType
        {
            Idle,
            Patrolling,
            Attacking,
        }

        public StateType currentState;

        public void UpdateState(StateType newstate)
        {
            currentState = newstate;
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}