using Fusion;
using UnityEngine;

namespace Photon
{
    public class NetworkBehaviourEx : NetworkBehaviour
    {
        /// <summary>
        /// 오브젝트 활성화 비활성화 동기화를 위한 함수
        /// </summary>
        /// <param name="value"></param>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetActiveRPC(bool value) => SetActiveSendRPC(value);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void SetActiveSendRPC(bool value)
        {
            gameObject.SetActive(value);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetPositionRPC(Vector3 pos) => transform.position = pos;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetRotationRPC(Quaternion quaternion) => transform.rotation = quaternion;
    }
}