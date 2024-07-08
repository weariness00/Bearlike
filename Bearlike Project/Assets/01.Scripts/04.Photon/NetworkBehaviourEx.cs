using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Photon
{
    public class NetworkBehaviourEx : NetworkBehaviour
    {
        [Networked, Capacity(3)] public NetworkArray<NetworkBool> IsAsyncClient { get; }
        [Networked] public NetworkBool IsSpawnSuccess { get; set; }
        
        public string SerializeComponentString(params Type[] components)
        {
            string componentString = "";
            foreach (var component in components)
            {
                componentString += $"{component.FullName},";
            }

            return componentString;
        }
        
        public List<Type> DeserializeComponentString(string componentString)
        {
            var split =  componentString.Split(",", StringSplitOptions.RemoveEmptyEntries);
            List<Type> components = new List<Type>();
            
            foreach (var s in split)
            {
                components.Add(Type.GetType(s));
            }

            return components;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void DestroyRPC(float time = 0f)
        {
            if(gameObject != null)
                Destroy(gameObject);
        }
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void DestroyRPC(NetworkId id, float time = 0f) => Destroy(Runner.FindObject(id), time);

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetParentRPC(NetworkId parentId)
        {
            var parentObj = Runner.FindObject(parentId);
            if (parentObj)
                transform.parent = parentObj.transform;
        }
        
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