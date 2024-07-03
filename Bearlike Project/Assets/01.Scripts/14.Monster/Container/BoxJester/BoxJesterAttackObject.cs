using Fusion;
using Photon;
using Status;
using UI.Status;
using Unity.VisualScripting;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAttackObject : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        public MonsterStatus status;

        public float damage;
        
        private void Awake()
        {
            status = gameObject.GetOrAddComponent<MonsterStatus>();

            status.damage.Max = (int)(damage);
            status.damage.Current = (int)(damage);
        }
        
        public override void Spawned()
        {
            // 지속 데미지임
            Destroy(gameObject, 2.0f);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (false == other.gameObject.CompareTag("Monster"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    status.AddAdditionalStatus(otherStatus);
                    // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                    //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                    otherStatus.ApplyDamageRPC(1, DamageTextType.Normal, OwnerId); 
                    status.RemoveAdditionalStatus(otherStatus);
                }
            }
        }
    }
}