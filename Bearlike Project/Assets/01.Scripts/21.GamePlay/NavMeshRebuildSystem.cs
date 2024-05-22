using Fusion;
using GamePlay.Stage;
using Photon;
using Unity.AI.Navigation;

namespace GamePlay
{
    public class NavMeshRebuildSystem : NetworkSingleton<NavMeshRebuildSystem>
    {
        private TickTimer _rebuildTimer;

        public override void Spawned()
        {
            _rebuildTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer &&
                _rebuildTimer.Expired(Runner))
            {
                _rebuildTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                ReBuildNavMesh();
            }
        }

        public void ReBuildNavMesh()
        {
            if (Runner.IsServer)
            {
                StageBase stage = GameManager.Instance.currentStage;
                if (stage)
                {
                    NavMeshSurface stageSurface = stage.navMeshSurface;
                    if (stageSurface)
                    {
                        stageSurface.RemoveData();
                        stageSurface.BuildNavMesh();
                    }
                }
            }
        }
    }
}

