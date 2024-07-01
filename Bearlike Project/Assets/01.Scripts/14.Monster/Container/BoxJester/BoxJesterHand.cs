using System;
using System.Collections.Generic;
using Fusion;
using Status;
using UI.Status;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterHand : MonoBehaviour
    {
        [Networked] public NetworkId OwnerId { get; set; }
        [SerializeField] private StatusBase status;

        private void Awake()
        {
            var root = transform.root.gameObject.GetComponent<NetworkObject>();
            OwnerId = root.Id;
        }

        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (false == other.gameObject.CompareTag("Monster"))//
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    status.AddAdditionalStatus(otherStatus);
                    // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                    //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                    otherStatus.ApplyDamageRPC(10, DamageTextType.Normal, OwnerId);
                    status.RemoveAdditionalStatus(otherStatus);
                }
            }
        }
    }
}