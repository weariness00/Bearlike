using UnityEngine;
using UnityEngine.AI;

namespace Test
{
    public class TestAgent : MonoBehaviour
    {
        public Transform targetTransform;
        public NavMeshAgent agent;
        public Rigidbody rigidbody;
        
        // Start is called before the first frame update
        void Start()
        {
            if (!agent) agent = GetComponent<NavMeshAgent>();
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if(agent.isOnNavMesh)
                agent.SetDestination(targetTransform.position);
        }
    }
}
