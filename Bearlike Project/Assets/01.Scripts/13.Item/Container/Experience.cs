using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class Experience : ItemBase
    {
        private int _expAmount = 0;
        
        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();

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
            if (other.gameObject.CompareTag("Player") && 
                other.TryGetComponent(out PlayerController pc) && pc.HasInputAuthority)
            {
                foreach (var sphereCollider in GetComponents<SphereCollider>())
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
            PlayerController pc;//  
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                pc.status.experience.Current += _expAmount;
            }
        }

        public void SetRandomExp(int min, int max) => _expAmount = Random.Range(min, max);
        public void SetExp(int value) => _expAmount = value;
        public int GetExp() => _expAmount;

        #endregion
    }
}
