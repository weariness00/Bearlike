using UnityEngine;

namespace BehaviorTree.Component
{
    public class BTStateComponent : MonoBehaviour
    {
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
    }
}