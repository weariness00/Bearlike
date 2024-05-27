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
        
        public float reBuildTime = 1f;

        private Coroutine _reBuildCoroutine;

        public void ReBuildNavMesh()
        {
            if (Runner.IsServer)
            {
                StageBase stage = GameManager.Instance.currentStage;
                if (stage && stage.navMeshSurface)
                {
                    NavMeshSurface stageSurface = stage.navMeshSurface;
                    if (_reBuildCoroutine == null)
                        _reBuildCoroutine = StartCoroutine(ReBuildCoroutine(stageSurface));
                }
            }
        }

        private IEnumerator ReBuildCoroutine(NavMeshSurface stageSurface)
        {
            yield return new WaitForSeconds(reBuildTime);
            
            DebugManager.ToDoError("원인 모를 이유로 인해 Physice Collider모드일때 베이크가 제대로 안된다. 고쳐야한다.");
            // stageSurface.RemoveData();
            // stageSurface.BuildNavMesh();
            
            _reBuildCoroutine = null;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ReBuildNavMeshRPC() => ReBuildNavMesh();
    }
}

