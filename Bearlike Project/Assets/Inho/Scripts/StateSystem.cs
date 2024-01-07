using UnityEngine;

namespace Inho.Scripts
{
    public class StateSystem : MonoBehaviour
    {
        public State ManagedState;

        private void Awake()
        {
            ManagedState = new PlayerState();   // adaptor 형식으로 바꿔보자
        }

        void Start()
        {
            ManagedState.Initialization();
        }

        void Update()
        {   
            ManagedState
        }
    } 
}

