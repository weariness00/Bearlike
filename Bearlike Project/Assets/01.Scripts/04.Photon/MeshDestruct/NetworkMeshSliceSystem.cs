using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Photon.MeshDestruct
{
    public class NetworkMeshSliceSystem : NetworkSingleton<NetworkMeshSliceSystem>
    {
        public NetworkPrefabRef sliceSocketObject;
        
        private async void Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint, float force = 1f, bool isChangTag = true, List<Type> componentList = null)
        {
            var obj = await Runner.SpawnAsync(sliceSocketObject);
            var socket = obj.GetComponent<NetworkMeshSliceSocket>();
            
            var sliceObjects = await socket.NetworkSlice(targetObject, sliceNormal, slicePoint, force, isChangTag, componentList);
            if (sliceObjects.Count == 1)
            {
                var sliceInfo = targetObject.GetComponent<NetworkMeshSliceObject>();
                sliceInfo.isSlice = false;
                Destroy(obj);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SliceRPC(NetworkId targetID, Vector3 sliceNormal, Vector3 slicePoint, float force = 1f, bool isChangeTag = true, string componentString = "")
        {
            var targetObject = Runner.FindObject(targetID).gameObject;
            var sliceInfo = targetObject.GetComponent<NetworkMeshSliceObject>();
            if(sliceInfo.isSlice)
                return;

            sliceInfo.isSlice = true;
            var components = DeserializeComponentString(componentString);
            Slice(targetObject, sliceNormal, slicePoint, force, isChangeTag, components);
        }
    }
}

