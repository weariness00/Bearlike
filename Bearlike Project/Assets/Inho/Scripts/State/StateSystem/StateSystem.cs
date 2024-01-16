using Inho.Scripts.Equipment;
using Inho.Scripts.State.StateClass;
using Inho.Scripts.State.StateClass.Pure;
using UnityEngine;

namespace Inho.Scripts.State.StateSystem
{
    /// <summary>
    /// 모든 State을 관리하는 System Class
    /// </summary>
    public class StateSystem : MonoBehaviour
    {
        private StateClass.Pure.State ManagedState;
        private EquitmentSystem Equitment;

        private void Awake()
        {
            ManagedState = new PlayerState();   // adapter 형식으로 바꿔보자
            Equitment = new EquitmentSystem();
        }

        void Start()
        {
            // ManagedState.Initialization();
            ManagedState.ShowInfo();
            Equitment.Init();
            InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }
        
        void Update()
        {   
            if(Input.GetKeyDown(KeyCode.Q)) ManagedState.ShowInfo();
            if(Input.GetKeyDown(KeyCode.E)) ManagedState.BeDamaged(Equitment.GetEquitment().GetDamage());
            if (Input.GetKeyDown(KeyCode.Z)) ManagedState.AddCondition((int)eCondition.Weak);
            if (Input.GetKeyDown(KeyCode.X)) ManagedState.DelCondition((int)eCondition.Weak);
            if(Input.GetKeyDown(KeyCode.C)) ManagedState.AddCondition((int)eCondition.Poisoned);
            if(Input.GetKeyDown(KeyCode.V)) ManagedState.DelCondition((int)eCondition.Poisoned);
        }

        private void MainLoop()
        {
            ManagedState.MainLoop();
        }
    } 
}

