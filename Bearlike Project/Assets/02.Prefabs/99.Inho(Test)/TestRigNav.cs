using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestRigNav : MonoBehaviour
{
    public GameObject player;
    public NavMeshAgent agent;
    public Rigidbody rig;
    
    void Start()
    {

    }

    
    void Update()
    {
        agent.SetDestination(player.transform.position);

        if (Input.GetKeyDown(KeyCode.T))
        {
            rig.AddForce((transform.position - player.transform.position).normalized);        
        }
        
    }
}
