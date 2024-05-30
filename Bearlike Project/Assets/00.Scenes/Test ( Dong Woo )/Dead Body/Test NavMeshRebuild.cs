using Unity.AI.Navigation;
using UnityEngine;

namespace Test
{
    public class TestNavMeshRebuild : MonoBehaviour
    {
        public NavMeshSurface Surface;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Surface.RemoveData();
                Surface.BuildNavMesh();
            }
        }
    }
}
