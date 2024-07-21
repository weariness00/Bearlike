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
            if (agent.isOnNavMesh)
            {
                Debug.Log(IsIncludeLink(targetTransform.position));
                agent.SetDestination(targetTransform.position);
            }
        }
        
        /// <summary>
        /// 경로에 Nav Mesh Link가 포함되어있는지 확인
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public bool IsIncludeLink(Vector3 targetPosition)
        {
            if (agent == null) return false;
            
            // 경로 계산
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPosition, path))
            {
                // 경로에 네비메쉬 링크가 포함되어 있는지 확인
                NavMeshHit hit;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    if (NavMesh.Raycast(path.corners[i], path.corners[i + 1], out hit, NavMesh.AllAreas))
                    {
                        if (hit.hit && hit.mask == NavMesh.GetAreaFromName("OffMeshLink"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
