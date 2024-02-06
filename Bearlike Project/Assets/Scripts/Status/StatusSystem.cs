using State.StateClass;
using State.StateClass.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace State
{
    /// <summary>
    /// 모든 State을 관리하는 System Class
    /// </summary>
    public class StatusSystem : MonoBehaviour
    {
        [FormerlySerializedAs("managedState")] [SerializeField] public StatusBase managedStatus;
        // private EquitmentSystem _equitment;
        public StatusBase Status => managedStatus;

        private void Awake()
        {
            if(gameObject.CompareTag("Player"))
            {
                managedStatus = gameObject.GetOrAddComponent<PlayerStatus>();
            }
            else
            {
                managedStatus = gameObject.GetOrAddComponent<MonsterStatus>();
            }
            // _equitment = new EquitmentSystem();
        }

        void Start()
        {
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
                InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name == "StateSystemScene")
            {
                if (gameObject.CompareTag("Player"))
                {
                    // if (Input.GetKeyDown(KeyCode.E)) managedState.ApplyDamage(_equitment.GetEquitment().GetDamage(), ObjectProperty.Normality);
                    if (Input.GetKeyDown(KeyCode.Z)) managedStatus.AddCondition(CrowdControl.Weak);
                    if (Input.GetKeyDown(KeyCode.X)) managedStatus.DelCondition(CrowdControl.Weak);
                    if (Input.GetKeyDown(KeyCode.C)) managedStatus.AddCondition(CrowdControl.Poisoned);
                    if (Input.GetKeyDown(KeyCode.V)) managedStatus.DelCondition(CrowdControl.Poisoned);
                }
            }
            if (Input.GetKeyDown(KeyCode.Q)) managedStatus.ShowInfo();
        }

        private void MainLoop()
        {
            managedStatus.MainLoop();
        }   

    } 
}

