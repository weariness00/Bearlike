using UnityEngine;

namespace Inho.Scripts.State
{
    /// <summary>
    /// 모든 State을 관리하는 System Class
    /// </summary>
    public class StateSystem : MonoBehaviour
    {
        public State ManagedState;

        private void Awake()
        {
            ManagedState = new PlayerState();   // adapter 형식으로 바꿔보자
        }

        void Start()
        {
            ManagedState.Initialization();
            ManagedState.ShowInfo();
        }
        
        void Update()
        {   
            if(Input.GetKeyDown(KeyCode.A)) ManagedState.ShowInfo();
            if(Input.GetKeyDown(KeyCode.S)) ManagedState.BeDamaged(1.0f);
            if (Input.GetKeyDown(KeyCode.Z)) ManagedState.AddCondition((int)eCondition.Weak);
            if (Input.GetKeyDown(KeyCode.X)) ManagedState.DelCondition((int)eCondition.Weak);
        }
    } 
}

