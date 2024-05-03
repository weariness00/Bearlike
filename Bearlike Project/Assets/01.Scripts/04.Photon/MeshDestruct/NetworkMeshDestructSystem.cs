using System;
using System.Collections;
using System.Linq;
using Data;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;

namespace Photon.MeshDestruct
{
    public class NetworkMeshDestructSystem : NetworkSingleton<NetworkMeshDestructSystem>
    {
        [Networked, Capacity(3)]private NetworkArray<NetworkBool> IsOtherClientDestruct { get;}
        public NetworkPrefabRef slicePrefab;
        public bool isDestruct;

        private GameObject _sliceTargetObject;
        
        public GameObject FindDestructObject(int id)
        {
            var destructObjects = FindObjectsOfType<NetworkMeshDestructObject>();
            var target = destructObjects.First(component => component.id == id);
            return target.gameObject;
        }

        public async void NetworkSlice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint)
        {
            if(!HasStateAuthority || !targetObject || isDestruct) return;
            isDestruct = true;
            
            var sliceObjects = MeshSlicing.Slice(targetObject, sliceNormal, slicePoint, false);

            if (sliceObjects.Count == 1)
            {
                isDestruct = false;
                return;
            }
            
            NetworkSliceInfo sliceInfo = default;
            sliceInfo.TargetId = targetObject.GetComponent<NetworkObject>().Id;
            sliceInfo.SliceNormal = sliceNormal;
            sliceInfo.SlicePoint = slicePoint;
            
            _sliceTargetObject = targetObject;
            targetObject.SetActive(false);
            
            for (var i = 0; i < sliceObjects.Count; i++)
            {
                var sliceObject = sliceObjects[i];
                var networkObj = await Runner.SpawnAsync(slicePrefab, sliceObject.transform.position, sliceObject.transform.rotation, null, (runner, o) =>
                {
                    o.tag = "Destruction";
                });

                var sliceMeshFilter = sliceObject.GetComponent<MeshFilter>();
                var networkMeshFilter = networkObj.GetComponent<MeshFilter>();
                var networkMeshRenderer = networkObj.GetComponent<MeshRenderer>();
                var networkMeshCollider = networkObj.GetComponent<MeshCollider>();
                var networkObjRigidBody = networkObj.GetComponent<Rigidbody>();

                networkMeshFilter.sharedMesh = sliceMeshFilter.sharedMesh;
                networkMeshRenderer.sharedMaterials = sliceObject.GetComponent<MeshRenderer>().sharedMaterials;
                networkMeshCollider.sharedMesh = sliceMeshFilter.sharedMesh;
                networkMeshCollider.convex = true;
                networkMeshCollider.enabled = true;
                networkObjRigidBody.useGravity = true;

                if(i == 0)
                    sliceInfo.SliceID0 = networkObj.Id;
                else
                    sliceInfo.SliceID1 = networkObj.Id;
                
                Destroy(sliceObject);
            }
            
            for (int i = 0; i < 3; i++)
            {
                IsOtherClientDestruct.Set(i, true);
            }
            
            var clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            StartCoroutine(SendSetIsDestructRPCCoroutine(clientNumber));
            NetworkSliceRPC(sliceInfo);
        }

        IEnumerator SendSetIsDestructRPCCoroutine(int clientNumber)
        {
            NetworkBool value = false;
            while (IsOtherClientDestruct.Get(clientNumber))
            {
                SetIsDestructRPC(clientNumber, value);
                yield return null;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void SetIsDestructRPC(int clientNumber, NetworkBool value)
        {
            IsOtherClientDestruct.Set(clientNumber, value);
            int count = Runner.ActivePlayers.ToArray().Length;
            for (int i = 0; i < 3; i++)
            {
                if (IsOtherClientDestruct.Get(i) == false)
                    --count;
            }

            if (count <= 0)
            {
                Destroy(_sliceTargetObject);
                isDestruct = false;
            }
        } 
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void NetworkSliceRPC(NetworkSliceInfo sliceInfo)
        {
            if(HasStateAuthority) return;
            
            var targetNetworkObject = Runner.FindObject(sliceInfo.TargetId);
            var targetObject = targetNetworkObject.gameObject;
            var sliceObjects = MeshSlicing.Slice(targetObject, sliceInfo.SliceNormal, sliceInfo.SlicePoint);
            targetObject.SetActive(false);
            
            GameObject[] networkObjects = new GameObject[2];
            networkObjects[0] = Runner.FindObject(sliceInfo.SliceID0).gameObject;
            networkObjects[1] = Runner.FindObject(sliceInfo.SliceID1).gameObject;
            for (int i = 0; i < 2; i++)
            {
                var networkObject = networkObjects[i];
                var networkObjMeshRenderer = networkObject.GetComponent<MeshRenderer>();
                var networkObjMeshFilter = networkObject.GetComponent<MeshFilter>();
                var networkObjMeshCollider = networkObject.GetComponent<MeshCollider>();
                var networkObjRigidBody = networkObject.GetComponent<Rigidbody>();
                    
                var sliceObject = sliceObjects[i];
                var sliceMeshRenderer = sliceObject.GetComponent<MeshRenderer>();
                var sliceMeshFilter = sliceObject.GetComponent<MeshFilter>();

                networkObjMeshRenderer.sharedMaterial = sliceMeshRenderer.sharedMaterial;
                networkObjMeshRenderer.sharedMaterials = sliceMeshRenderer.sharedMaterials;
                networkObjMeshFilter.sharedMesh = sliceMeshFilter.sharedMesh;
                networkObjMeshCollider.sharedMesh = sliceMeshFilter.sharedMesh;
                networkObjMeshCollider.convex = true;
                networkObjMeshCollider.enabled = true;
                networkObjRigidBody.useGravity = true;
                    
                Destroy(sliceObject);
            }
            
            // 해당 클라이언트의 붕괴가 끝났음을 호스트에게 알리기
            var clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            StartCoroutine(SendSetIsDestructRPCCoroutine(clientNumber));
        }
    }
}

