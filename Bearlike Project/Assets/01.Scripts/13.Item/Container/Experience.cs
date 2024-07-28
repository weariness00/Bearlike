using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class Experience : ItemBase
    {
        private Collider _collider;
        
        private int _expAmount = 0;
        
        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();

            _collider = GetComponent<Collider>();
            _collider.enabled = false;
            
            Invoke(nameof(ActiveCollider), 2f);

            var statusData = GetStatusData(Id);
            int defaultAmount = statusData.GetInt("Experience Default Amount");
            int minAmount = statusData.GetInt("Experience Min Amount");
            int maxAmount = statusData.GetInt("Experience Max Amount");

            _expAmount = defaultAmount + Random.Range(minAmount, maxAmount);
        }

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 360f,0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CheckPlayer(other.gameObject, out PlayerController pc))
            {
                foreach (var sphereCollider in GetComponentsInChildren<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion

        #region Member Function

        public override void GetItem(GameObject targetObject)
        {
            PlayerController pc;
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                pc.status.IncreaseExp(_expAmount);
                pc.soundController.PlayItemEarn();
            }
        }
        
        void ActiveCollider()
        {
            _collider.enabled = true;
        }

        public void SetRandomExp(int min, int max) => _expAmount = Random.Range(min, max);
        public void SetExp(int value) => _expAmount = value;
        public int GetExp() => _expAmount;

        #endregion
    }
}
