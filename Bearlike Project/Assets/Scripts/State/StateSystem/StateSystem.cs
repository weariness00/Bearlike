using Inho.Scripts.Equipment;
using State.StateClass;
using State.StateClass.Base;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace State.StateSystem
{
    /// <summary>
    /// 모든 State을 관리하는 System Class
    /// </summary>
    public class StateSystem : MonoBehaviour
    {
        [SerializeField] public global::State.StateClass.Base.State managedState;
        private EquitmentSystem _equitment;

        private void Awake()
        {
            if(gameObject.CompareTag("Player"))
                managedState = gameObject.AddComponent<PlayerState>();
            _equitment = new EquitmentSystem();
        }

        void Start()
        {
            _equitment.Init();
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
                InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
            {
                if (Input.GetKeyDown(KeyCode.E)) managedState.ApplyDamage(_equitment.GetEquitment().GetDamage(), ObjectProperty.Normality);
                if (Input.GetKeyDown(KeyCode.Z)) managedState.AddCondition(ObjectProperty.Weak);
                if (Input.GetKeyDown(KeyCode.X)) managedState.DelCondition(ObjectProperty.Weak);
                if (Input.GetKeyDown(KeyCode.C)) managedState.AddCondition(ObjectProperty.Poisoned);
                if (Input.GetKeyDown(KeyCode.V)) managedState.DelCondition(ObjectProperty.Poisoned);
            }
            if (Input.GetKeyDown(KeyCode.Q)) managedState.ShowInfo();
        }

        private void MainLoop()
        {
            managedState.MainLoop();
        }   

        public global::State.StateClass.Base.State GetState() { return managedState; }
    } 
}

