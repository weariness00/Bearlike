using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class Experience : ItemBase
    {
        #region Unity Event Function

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 360f,0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && other.transform.parent.name == "Local Player")
            {
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion
    }
}
