using UnityEngine;

namespace Inho.Scripts.State
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
            ManagedState.ShowInfo();
        }

        void Update()
        {   
            if(Input.GetKeyDown(KeyCode.A)) ManagedState.ShowInfo();
            if(Input.GetKeyDown(KeyCode.D)) ManagedState.BeDamaged(10);
        }
    } 
}

