using System;
using System.Collections.Generic;
using Fusion;
using Manager;
using Photon;
using Player;
using UnityEngine;

namespace GamePlay
{
    public class JumpPad : NetworkBehaviourEx
    {
        public float jumpPower = 1f;

        private bool _isServer;

        private void OnTriggerEnter(Collider other)
        {
            if (_isServer)
            {
                Rigidbody rb = other.attachedRigidbody;
                if (other.TryGetComponent(out ColliderStatus cs))
                    rb = cs.originalStatus.GetComponent<Rigidbody>();
                else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    var pc = other.transform.root.GetComponent<PlayerController>();
                    pc.simpleKcc.Move(Vector3.zero, jumpPower * Vector3.up);
                }
                
                if(rb) rb.AddForce(jumpPower * Vector3.up);
            }
        }
        
        public override void Spawned()
        {
            _isServer = Runner.IsServer;
        }
    }
}

