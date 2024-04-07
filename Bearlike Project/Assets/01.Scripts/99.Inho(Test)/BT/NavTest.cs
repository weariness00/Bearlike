using UnityEngine;
using UnityEngine.AI;

public class NavTest : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject agentGameObject;
    private NavMeshAgent _agent;
    
    void Start()
    {
        _agent = agentGameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _agent.SetDestination(player.transform.position);
        // Debug.Log($"{agentGameObject.transform.position}");   
    }
}
