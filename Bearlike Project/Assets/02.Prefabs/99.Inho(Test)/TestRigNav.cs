using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class TestRigNav : MonoBehaviour
{
    public GameObject player;
    public NavMeshAgent agent;
    public Rigidbody rig;
    
    public float forceY = 0;
        
    void Start()
    {

    }

    
    void Update()
    {
        agent.SetDestination(player.transform.position);

        if (Input.GetKeyDown(KeyCode.T))
        {
            agent.enabled = false;

            
            Vector3 knockbackDirection = transform.position - player.transform.position;
            knockbackDirection.y = forceY;
            knockbackDirection.Normalize();
            Debug.Log(knockbackDirection);
            
            transform.DOMove(transform.position + knockbackDirection, 0.5f).SetEase(Ease.OutCirc);
            
            // rig.AddForce(knockbackDirection * 10, ForceMode.Impulse);
            
            StartCoroutine(ASD());
        }
        
    }
    
    IEnumerator ASD()
    {
        yield return new WaitForSeconds(0.5f);
        // rig.velocity = new Vector3(0, 0, 0);
        agent.enabled = true;   
    }
    
}
