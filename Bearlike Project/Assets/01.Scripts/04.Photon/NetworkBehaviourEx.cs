using Fusion;
using UnityEngine;

namespace Photon
{
    public class NetworkBehaviourEx : NetworkBehaviour
    {
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DestroyRPC(NetworkId id, float time = 0f) => Destroy(Runner.FindObject(id), time);
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetActiveRPC(NetworkBool value) => gameObject.SetActive(value);
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetPositionRPC(Vector3 pos) => transform.position = pos;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetRotationRPC(Quaternion quaternion) => transform.rotation = quaternion;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void LookAtRPC(NetworkId id) => transform.LookAt(Runner.FindObject(id).transform);
    }
}