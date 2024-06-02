using System.Collections;
using Fusion;
using GamePlay.Stage;
using Manager;
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
            if (Runner.IsServer)
            {
                if (surface)
                {
                    _reBuildCoroutine = StartCoroutine(ReBuildCoroutine(surface));
                }
            }
        }

        private IEnumerator ReBuildCoroutine(NavMeshSurface stageSurface)
        {
            yield return new WaitForSeconds(reBuildTime);
            
            DebugManager.ToDoError("원인 모를 이유로 인해 Physice Collider모드일때 베이크가 제대로 안된다. 고쳐야한다.");
            // Physics.SyncTransforms();
            // stageSurface.RemoveData();
            // stageSurface.BuildNavMesh();
            
            _reBuildCoroutine = null;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ReBuildNavMeshRPC() => ReBuildNavMesh();
    }
}

