using Fusion;

namespace Inho_Test_.PhotonPractice
{
    public class TestPlayer : NetworkBehaviour
    {
        private NetworkCharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out TestNetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(5*data.direction*Runner.DeltaTime);
            }
        }
    }
}
