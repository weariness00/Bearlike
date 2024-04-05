﻿using System;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Photon
{
    public class NetworkBehaviourEx : NetworkBehaviour
    {
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
    }
}