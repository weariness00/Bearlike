using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDomino : MonoBehaviour
{
    public Rigidbody rb;
    
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"OnCollisionEnter : {other.gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter : {other.gameObject.name}");

    }

    private void Start()
    {
        rb.AddTorque(Vector3.right * 100);
    }
}
