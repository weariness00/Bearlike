using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class TestDeadBodyGravityField : MonoBehaviour
    {
        public float gravityPower = 100f;
        public float rotateStrength = 1f;

        private List<Rigidbody> objectlist = new List<Rigidbody>();
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody)
            {
                objectlist.Add(other.attachedRigidbody);
            }
        }

        public void Update()
        {
            foreach (var o in objectlist)
            {
                Vector3 dir = transform.position - o.transform.position;
                Vector3 force = gravityPower * dir.normalized;
                
                Vector3 torqueDirection = Vector3.Cross(dir, Vector3.up);
                Vector3 rotationalForce = torqueDirection * rotateStrength;
                
                o.AddForce(force);
                o.AddTorque(rotationalForce);
            }
        }
    }
}
