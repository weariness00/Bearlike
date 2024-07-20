using Fusion;
using Photon;
using Status;
using UI.Status;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAttackHand : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        
        [SerializeField] private StatusBase status;
        [SerializeField]private GameObject[] hands;

        public Vector3 targetPosition;
        
        private void Awake()
        {
            var root = transform.root.gameObject.GetComponent<NetworkObject>();
            OwnerId = root.Id;

            status.damage.Max = 10;
            status.damage.Current = 10;
        }

        public override void Spawned()
        {
            base.Spawned();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (true == other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    status.AddAdditionalStatus(otherStatus);
                    // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                    //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                    otherStatus.ApplyDamageRPC(status.damage.Current, DamageTextType.Normal, OwnerId);
                    status.RemoveAdditionalStatus(otherStatus);
                }
            }
        }
    }
}