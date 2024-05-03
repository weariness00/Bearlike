using Player;
using UnityEngine;

namespace Item.Container
{
    public class Money : ItemBase
    {
        #region Unity Event Function

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 90f,0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && 
                (other.TryGetComponent(out PlayerController pc) || other.transform.root.TryGetComponent(out pc)) && pc.HasInputAuthority)
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

