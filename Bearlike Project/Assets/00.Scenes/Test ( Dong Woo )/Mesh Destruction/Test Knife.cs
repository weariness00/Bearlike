using System;
using UnityEngine;
using Util;

namespace Test
{
    public class TestKnife : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            var contact = other.contacts[0];
            MeshSlicing.Slice(other.gameObject,transform.up, contact.point);
            
            Debug.Log("Slice");
        }
    }
}