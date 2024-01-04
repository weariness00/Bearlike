using Fusion;
using Script.Photon;
using UnityEngine;

namespace Script.Player
{
    public class PlayerController : NetworkBehaviour
    {
        private NetworkCharacterController _cc;
    
        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }
    
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(5*data.direction*Runner.DeltaTime);
            }
        }
    }
}