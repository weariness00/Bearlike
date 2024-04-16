using System;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Photon
{
    public class NetworkBehaviourEx : NetworkBehaviour
    {
        [Networked, Capacity(3)] public NetworkArray<NetworkBool> IsAsyncClient { get; }
        [Networked] public NetworkBool IsSpawnSuccess { get; set; }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void DestroyRPC(float time = 0f)
        {
            if(gameObject != null)
                Destroy(gameObject);
        }
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void DestroyRPC(NetworkId id, float time = 0f) => Destroy(Runner.FindObject(id), time);
        
        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void SetActiveRPC(NetworkBool value) => gameObject.SetActive(value);
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetPositionRPC(Vector3 pos) => transform.position = pos;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetRotationRPC(Quaternion quaternion) => transform.rotation = quaternion;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void LookAtRPC(NetworkId id) => transform.LookAt(Runner.FindObject(id).transform);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void DespawnRPC()
        {
            Debug.Log("정상 삭제됨");
            Runner.Despawn(Object);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SpawnedSuccessRPC(int clientNumber, NetworkBool value)
        {
            IsAsyncClient.Set(clientNumber, value);

            var count = Runner.ActivePlayers.ToArray().Length;
            foreach (var asyncValue in IsAsyncClient)
            {
                if (asyncValue) --count;
            }

            if (count == 0) IsSpawnSuccess = true;
        }
    }
}