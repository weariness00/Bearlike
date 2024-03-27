using UnityEngine;
using UnityEngine.AI;

public class NavTest : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public NavMeshAgent agent;
    
    void Start()
    {
    }

    void Update()
    {
        agent.SetDestination(player.transform.position);
    }
}
