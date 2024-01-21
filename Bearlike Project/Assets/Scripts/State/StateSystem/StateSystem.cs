using Inho.Scripts.Equipment;
using State.StateClass;
using State.StateClass.Pure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace State.StateSystem
{
    /// <summary>
    /// 모든 State을 관리하는 System Class
    /// </summary>
    public class StateSystem : MonoBehaviour
    {
        private global::State.StateClass.Pure.State mManagedState;
        private EquitmentSystem mEquitment;

        private void Awake()
        {
            if(gameObject.CompareTag("Player"))
                mManagedState = new PlayerState();
            mEquitment = new EquitmentSystem();
        }

        void Start()
        {
            mEquitment.Init();
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
                InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
            {
                if (Input.GetKeyDown(KeyCode.E)) mManagedState.BeDamaged(mEquitment.GetEquitment().GetDamage());
                if (Input.GetKeyDown(KeyCode.Z)) mManagedState.AddCondition((int)eCondition.Weak);
                if (Input.GetKeyDown(KeyCode.X)) mManagedState.DelCondition((int)eCondition.Weak);
                if (Input.GetKeyDown(KeyCode.C)) mManagedState.AddCondition((int)eCondition.Poisoned);
                if (Input.GetKeyDown(KeyCode.V)) mManagedState.DelCondition((int)eCondition.Poisoned);
            }
            if (Input.GetKeyDown(KeyCode.Q)) mManagedState.ShowInfo();
        }

        private void MainLoop()
        {
            mManagedState.MainLoop();
        }   

        public global::State.StateClass.Pure.State GetState() { return mManagedState; }
    } 
}

