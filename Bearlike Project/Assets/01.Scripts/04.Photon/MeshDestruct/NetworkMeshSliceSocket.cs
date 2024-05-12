using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Fusion;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using Util;

namespace Photon.MeshDestruct
{
    public class NetworkMeshSliceSocket : NetworkBehaviourEx
    {
        [Networked, Capacity(3)]public NetworkArray<NetworkBool> IsOtherClientDestruct { get;}
        public NetworkPrefabRef slicePrefab;
        public NetworkPrefabRef copyPrefab; // Slice 되기전 Mesh를 Copy하는 프리펩

        private bool _isCallSpawned = false;
        private IEnumerator _enumeratorSendSetIsDestructRPCCoroutine;
        
        private GameObject _sliceTargetObject;
        private GameObject _copyTargetObject;
        private bool _isInitCopy;

        public void Update()
        {
            if(!Object)
                Destroy(gameObject);
        }

        public override void Spawned()
        {
            base.Spawned();
            _isCallSpawned = true;
            if (_enumeratorSendSetIsDestructRPCCoroutine != null)
                StartCoroutine(_enumeratorSendSetIsDestructRPCCoroutine);
            
            var clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            SpawnedSuccessRPC(clientNumber, true);
        }

        public async Task<List<GameObject>> NetworkSlice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint, float force = 1f, bool isChangTag = true , List<Type> components = null)
        {
            if(!HasStateAuthority || !targetObject) return new List<GameObject>(){targetObject};
            if(targetObject.TryGetComponent(out NetworkMeshSliceObject meshSliceObject) == false || meshSliceObject.isHasMesh == false) return new List<GameObject>(){targetObject};
            
            var sliceObjects = MeshSlicing.Slice(targetObject, sliceNormal, slicePoint, false);

            if (sliceObjects.Count == 1)
                return new List<GameObject>(){targetObject};
            
            // Slice하려는 위치와 현재위치의 차이로 힘의 방향 얻기
            var targetID = targetObject.GetComponent<NetworkObject>().Id;
            var copyObj = await Runner.SpawnAsync(copyPrefab, targetObject.transform.position, targetObject.transform.rotation);
            while (true)
            {
                if (IsSpawnSuccess)
                    break;
                await Task.Delay(10);
            }
            
            InitCopyObjectRPC(targetID, copyObj.Id);
            
            NetworkSliceInfo sliceInfo = default;
            sliceInfo.TargetId = copyObj.Id;
            sliceInfo.SliceNormal = sliceNormal;
            sliceInfo.SlicePoint = slicePoint;
            //
            _sliceTargetObject = targetObject;
            _copyTargetObject = copyObj.gameObject;
            
            List<GameObject> networkSliceObjectList = new List<GameObject>();
            Vector3 forceDirection = (slicePoint - targetObject.transform.position).normalized;
            for (var i = 0; i < sliceObjects.Count; i++)
            {
                var sliceObject = sliceObjects[i];
                var networkObj = await Runner.SpawnAsync(slicePrefab, sliceObject.transform.position, sliceObject.transform.rotation, null, (runner, o) =>
                {
                    if(isChangTag) o.tag = "Destruction";
                    o.AddComponent<NetworkMeshSliceObject>();
                    if (components != null)
                    {
                        foreach (Type component in components)
                            o.gameObject.AddComponent(component);
                    }
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
                networkObjRigidBody.AddForce(force * 220f * forceDirection);

                if(i == 0)
                    sliceInfo.SliceID0 = networkObj.Id;
                else
                    sliceInfo.SliceID1 = networkObj.Id;
                
                Destroy(sliceObject);
                
                networkSliceObjectList.Add(networkObj.gameObject);
            }
            
            for (int i = 0; i < 3; i++)
            {
                IsOtherClientDestruct.Set(i, true);
            }
            
            var clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            SetIsDestructRPC(clientNumber, false);
            NetworkSliceRPC(sliceInfo);

            return networkSliceObjectList;
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

        IEnumerator InitCopyObjectCoroutine(NetworkId targetID, NetworkId copyID)
        {
            GameObject copy, target;
            while (true)
            {
                var netObj = Runner.FindObject(targetID);
                if (netObj)
                {
                    target = netObj.gameObject;
                    break;
                }
                yield return null;
            }
            while (true)
            {
                var netObj = Runner.FindObject(copyID);
                if (netObj)
                {
                    copy = netObj.gameObject;
                    break;
                }
                yield return null;
            }
            copy.SetActive(true);
            var meshFilter = copy.GetComponent<MeshFilter>();
            var meshRenderer = copy.GetComponent<MeshRenderer>();
            var targetMeshRenderer = target.GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;
            meshRenderer.sharedMaterials = targetMeshRenderer.sharedMaterials;
            meshRenderer.enabled = false;
            targetMeshRenderer.enabled = false;

            _isInitCopy = true;
        }
        
        IEnumerator SpawnSliceObjectCoroutine(NetworkSliceInfo sliceInfo)
        {
            while (_isInitCopy == false)
                yield return null;
            
            var targetObject = Runner.FindObject(sliceInfo.TargetId).gameObject;
            var sliceObjects = MeshSlicing.Slice(targetObject, sliceInfo.SliceNormal, sliceInfo.SlicePoint, false);
            
            GameObject[] networkObjects = new GameObject[2];
            while (true)
            {
                var obj = Runner.FindObject(sliceInfo.SliceID0);
                if (obj)
                {
                    networkObjects[0] = obj.gameObject;
                    break;
                }
                yield return null;
            }
            while (true)
            {
                var obj = Runner.FindObject(sliceInfo.SliceID1);
                if (obj)
                {
                    networkObjects[1] = obj.gameObject;
                    break;
                }
                yield return null;
            }
            
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
            _enumeratorSendSetIsDestructRPCCoroutine = SendSetIsDestructRPCCoroutine(clientNumber);
            if (_isCallSpawned)
            {
                StartCoroutine(_enumeratorSendSetIsDestructRPCCoroutine);
                _enumeratorSendSetIsDestructRPCCoroutine = null;
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
                Destroy(_copyTargetObject);
                Destroy(gameObject);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void InitCopyObjectRPC(NetworkId targetID, NetworkId copyID)
        {
            StartCoroutine(InitCopyObjectCoroutine(targetID, copyID));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void NetworkSliceRPC(NetworkSliceInfo sliceInfo)
        {
            if(HasStateAuthority) return;

            StartCoroutine(SpawnSliceObjectCoroutine(sliceInfo));
        }
    }
}

