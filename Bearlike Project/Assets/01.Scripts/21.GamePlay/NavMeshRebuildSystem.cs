using System.Collections;
using Fusion;
using Photon;
using Unity.AI.Navigation;
using UnityEngine;

namespace GamePlay
{
    public class NavMeshRebuildSystem : NetworkSingleton<NavMeshRebuildSystem>
    {
        public static void ReBuild() => Instance.ReBuildNavMesh();
        public static void ReBuildRPC() => Instance.ReBuildNavMeshRPC();

        public static void SetSurface(NavMeshSurface s) => Instance.surface = s;
        
        public float reBuildTime = 1f;

        private Coroutine _reBuildCoroutine;
        [SerializeField] private NavMeshSurface surface;

        public void ReBuildNavMesh()
        {
            if (Runner.IsServer && surface)
            {
                _reBuildCoroutine ??= StartCoroutine(ReBuildCoroutine(surface));
            }
        }

        private IEnumerator ReBuildCoroutine(NavMeshSurface stageSurface)
        {
            yield return new WaitForSeconds(reBuildTime);
            
            stageSurface.RemoveData();
            stageSurface.BuildNavMesh();
            
            _reBuildCoroutine = null;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ReBuildNavMeshRPC() => ReBuildNavMesh();
    }
}

