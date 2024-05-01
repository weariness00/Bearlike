﻿using System;
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
        
        public async void NetworkDestruct(GameObject targetObject, PrimitiveType shapeType, Vector3 position, Vector3 size, Vector3 force)
        {
            // 상태권한이 있지 않으면 붕괴 못하게 하기
            if(!HasStateAuthority || !targetObject || isDestruct) return;

            isDestruct = true;
            StartCoroutine(NetworkDestructCoroutine(targetObject, shapeType, position, size, force));
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
                    o.transform.parent = sliceObject.transform.parent;
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
            SetIsDestructRPC(0, false);
            NetworkSliceRPC(sliceInfo);
        }

        private IEnumerator NetworkDestructCoroutine(GameObject targetObject, PrimitiveType shapeType, Vector3 position, Vector3 size, Vector3 force)
        {
            var networkMeshDestructObject = targetObject.GetComponent<NetworkMeshDestructObject>();
            var destructObjects = MeshDestruction.Destruction(targetObject, shapeType, position, size);
            
            // 붕괴된 객체가 없을 시
            if (destructObjects == Array.Empty<GameObject>())
            {
                isDestruct = false;
                yield break;
            }

            yield return null;
            
            // 0 번째 원소가 겹치는 영역              => 부서진 부분
            // 1 번째 원소가 겹치지 않는 영역          => 남아있는 부분
            NetworkDestructInfo destructInfo = new NetworkDestructInfo()
            {
                Id = networkMeshDestructObject.id,
                ShapeType = shapeType,
                ShapePosition = position,
                ShapeSize = size,
                
                SliceNormal = Random.onUnitSphere.normalized,
            };
            
            { // 남아있는 부분
                var obj = destructObjects[1];
                var meshFilter = obj.GetComponent<MeshFilter>();
                var meshCollider = obj.AddComponent<MeshCollider>();
                var info = obj.AddComponent<NetworkMeshDestructObject>();

                meshCollider.sharedMesh = meshFilter.sharedMesh;

                info.tag = "Destruction";
            }
            
            { // 부서진 부분
                var obj = destructObjects[0];
                var sliceObjects = MeshSlicing.Slice(obj, destructInfo.SliceNormal, destructInfo.ShapePosition);
                if (sliceObjects.Count == 1)
                {
                    var interactObject = sliceObjects[0];
                    var networkObjectOp = Runner.SpawnAsync(slicePrefab, interactObject.transform.position, interactObject.transform.rotation, null, (runner, o) =>
                    {
                        o.transform.parent = interactObject.transform.parent;
                    });

                    while (networkObjectOp.IsSpawned == false)
                        yield return null;
                    NetworkObject networkObject = networkObjectOp.Object;
                    
                    var networkObjMeshRenderer = networkObject.GetComponent<MeshRenderer>();
                    var networkObjMeshFilter = networkObject.GetComponent<MeshFilter>();
                    var networkObjMeshCollider = networkObject.GetComponent<MeshCollider>();
                    var networkObjRigidBody = networkObject.GetComponent<Rigidbody>();
                    
                    var interactMeshRenderer = interactObject.GetComponent<MeshRenderer>();
                    var interactMeshFilter = interactObject.GetComponent<MeshFilter>();

                    networkObjMeshRenderer.sharedMaterial = interactMeshRenderer.sharedMaterial;
                    networkObjMeshRenderer.sharedMaterials = interactMeshRenderer.sharedMaterials;
                    networkObjMeshFilter.sharedMesh = interactMeshFilter.sharedMesh;
                    networkObjMeshCollider.sharedMesh = interactMeshFilter.sharedMesh;
                    networkObjMeshCollider.convex = true;
                    networkObjMeshCollider.enabled = true;

                    networkObjRigidBody.useGravity = true;
                    networkObjRigidBody.AddForce(force * 100);

                    destructInfo.InteractObjectId = networkObject.Id;
                    
                    Destroy(interactObject);
                    
                    yield return null;
                }
                else if (sliceObjects.Count == 2)
                {
                    NetworkId[] sliceIds = new NetworkId[2];
                    for (int i = 0; i < 2; i++)
                    {
                        var sliceObject = sliceObjects[i];
                        var networkObjectOp = Runner.SpawnAsync(slicePrefab, sliceObject.transform.position, sliceObject.transform.rotation, null, (runner, o) =>
                        {
                            o.transform.parent = sliceObject.transform.parent;
                        });
                        
                        while (networkObjectOp.IsSpawned == false)
                            yield return null;
                        NetworkObject networkObject = networkObjectOp.Object;
                        
                        sliceIds[i] = networkObject.Id;
                    
                        var networkObjMeshRenderer = networkObject.GetComponent<MeshRenderer>();
                        var networkObjMeshFilter = networkObject.GetComponent<MeshFilter>();
                        var networkObjMeshCollider = networkObject.GetComponent<MeshCollider>();
                        var networkObjRigidBody = networkObject.GetComponent<Rigidbody>();
                    
                        var sliceMeshRenderer = sliceObject.GetComponent<MeshRenderer>();
                        var sliceMeshFilter = sliceObject.GetComponent<MeshFilter>();

                        networkObjMeshRenderer.sharedMaterial = sliceMeshRenderer.sharedMaterial;
                        networkObjMeshRenderer.sharedMaterials = sliceMeshRenderer.sharedMaterials;
                        networkObjMeshFilter.sharedMesh = sliceMeshFilter.sharedMesh;
                        networkObjMeshCollider.sharedMesh = sliceMeshFilter.sharedMesh;
                        networkObjMeshCollider.convex = true;
                        networkObjMeshCollider.enabled = true;

                        networkObjRigidBody.useGravity = true;
                        networkObjRigidBody.AddForce(force * 100);
                    
                        Destroy(sliceObject);
                        yield return null;
                    }

                    destructInfo.SliceObjectId0 = sliceIds[0];
                    destructInfo.SliceObjectId1 = sliceIds[1];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                IsOtherClientDestruct.Set(i, true);
            }
            SetIsDestructRPC(0, false);
            NetworkDestructRPC(destructInfo);
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
        
        /// <summary>
        /// 받은 정보를 토대로 메쉬를 잘라주는 RPC
        /// </summary>
        /// <param name="destructInfo"></param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void NetworkDestructRPC(NetworkDestructInfo destructInfo)
        {
            if(HasStateAuthority) return;

            StartCoroutine(NetworkDestructCoroutine(destructInfo));
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
            SetIsDestructRPC(clientNumber, false);
        }
        
        private IEnumerator NetworkDestructCoroutine(NetworkDestructInfo destructInfo)
        {
            var targetObj = FindDestructObject(destructInfo.Id);
            var destructObjects = MeshDestruction.Destruction(targetObj, destructInfo.ShapeType, destructInfo.ShapePosition, destructInfo.ShapeSize);
            yield return null;
            // 0 번째 원소가 겹치는 영역              => 부서진 부분
            // 1 번째 원소가 겹치지 않는 영역          => 남아있는 부분

            { // 0 번재 원소 슬라이싱 후 컴포넌트 조정
                var obj = destructObjects[0];
                var sliceObjects = MeshSlicing.Slice(obj, destructInfo.SliceNormal, destructInfo.ShapePosition);
                
                if (sliceObjects.Count == 1)
                {
                    var interactObject = sliceObjects[0];
                    var networkObject = Runner.FindObject(destructInfo.InteractObjectId).gameObject;
                    
                    var networkObjMeshRenderer = networkObject.GetComponent<MeshRenderer>();
                    var networkObjMeshFilter = networkObject.GetComponent<MeshFilter>();
                    var networkObjMeshCollider = networkObject.GetComponent<MeshCollider>();
                    var networkObjRigidBody = networkObject.GetComponent<Rigidbody>();
                    
                    var interactMeshRenderer = interactObject.GetComponent<MeshRenderer>();
                    var interactMeshFilter = interactObject.GetComponent<MeshFilter>();

                    networkObjMeshRenderer.sharedMaterial = interactMeshRenderer.sharedMaterial;
                    networkObjMeshRenderer.sharedMaterials = interactMeshRenderer.sharedMaterials;
                    networkObjMeshFilter.sharedMesh = interactMeshFilter.sharedMesh;
                    networkObjMeshCollider.sharedMesh = interactMeshFilter.sharedMesh;
                    networkObjMeshCollider.convex = true;
                    networkObjMeshCollider.enabled = true;

                    networkObjRigidBody.useGravity = true;
                    
                    Destroy(interactObject);
                }
                else if (sliceObjects.Count == 2)
                {
                    GameObject[] networkObjects = new GameObject[2];
                    networkObjects[0] = Runner.FindObject(destructInfo.SliceObjectId0).gameObject;
                    networkObjects[1] = Runner.FindObject(destructInfo.SliceObjectId1).gameObject;

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
                        yield return null;
                    }
                }
            }

            { // 1번쨰 원소 컴포넌트 조정
                var obj = destructObjects[1];
                var meshFilter = obj.GetComponent<MeshFilter>();
                var meshCollider = obj.AddComponent<MeshCollider>();
                var info = obj.AddComponent<NetworkMeshDestructObject>();

                meshCollider.sharedMesh = meshFilter.sharedMesh;
                
                info.id = destructInfo.Id;
                info.tag = "Destruct";
            }

            // 해당 클라이언트의 붕괴가 끝났음을 호스트에게 알리기
            var clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            SetIsDestructRPC(clientNumber, false);
        }
    }
}

