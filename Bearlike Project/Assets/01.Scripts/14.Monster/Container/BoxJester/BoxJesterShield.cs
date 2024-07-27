using Fusion;
using Status;
using UI.Status;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterShield : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;
        
            if (true == other.gameObject.CompareTag("Player"))
            {
                var id = transform.root.GetComponent<NetworkObject>().Id;
                
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    otherStatus.ApplyDamageRPC(25, DamageTextType.Normal, id);
                }
            }
        }
    }
}