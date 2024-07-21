using Player;
using UnityEngine;
using Weapon.Gun;

namespace Item.Container
{
    public class MagazineCartridge : ItemBase
    {
        private void OnTriggerEnter(Collider other)
        {
            if(GunBase.ammo.isMax) return;
            
            if (CheckPlayer(other.gameObject, out var pc))
            {
                rigidbody.isKinematic = true;
                
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        public override void GetItem(GameObject targetObject)
        {   
            PlayerController pc;
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                GunBase.ammo.Current += Amount.Current;
                pc.soundController.PlayItemEarn();
            }
        }
    }
}