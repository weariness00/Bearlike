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
                rigidbody.isKinematic = true;
                
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion

        #region Inventory Interface

        public override ItemBase UseItem<UseItem>(UseItem useItem, out bool isDestroy)
        {
            isDestroy = false;
            if (useItem is ItemBase item)
            {
                Amount.Current -= item.Amount.Current;

                if (Amount.isMin)
                {
                    Destroy(gameObject);
                    isDestroy = true;
                }
            }

            return this;
        }

        #endregion
    }
}

