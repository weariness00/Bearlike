using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Util;

namespace Photon
{
    public class NetworkMeshDestructSystem : NetworkSingleton<NetworkMeshDestructSystem>
    {
        public static readonly uint SendByteCapacity = 5000;
        
        [Networked, Capacity(3)] public NetworkDictionary<PlayerRef, NetworkBool> IsSuccessDestructionDict { get; }
        public NetworkPrefabRef emptyPrefab;
        public NetworkPrefabRef socketPrefab; // 송수신 용도로 사용할 네트워크 객체, 트래픽을 줄이고 코드를 단순화 하기 위해 사용

        public override void Spawned()
        {
            AddIsSuccessDestructionDictRPC(Runner.LocalPlayer);
        }

        public void SetIsSuccessDestructionDict(PlayerRef playerRef, bool value) => StartCoroutine(SetIsSuccessDestructionDictCoroutine(playerRef, value));

        public IEnumerator SetIsSuccessDestructionDictCoroutine(PlayerRef playerRef, bool value)
        {
            while (IsSuccessDestructionDict.Get(playerRef) == false)
            {
                SetIsSuccessDestructionDictRPC(Runner.LocalPlayer, true);
                yield return null;
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void DestructRPC(NetworkId id, PrimitiveType shapeType, Vector3 position, Vector3 size, Vector3 force)
        {
            // 현재 모든 클라이언트가 붕괴중이면 해당 함수 진행 안하게 하기
            foreach (var (playerRef, value) in IsSuccessDestructionDict)
            {
                if (value == false)
                {
                    return;
                }
            }
            foreach (var (playerRef, value) in IsSuccessDestructionDict)
            {
                IsSuccessDestructionDict.Set(playerRef, false);
            }

            var targetObject = Runner.FindObject(id).gameObject;
            var objects = MeshDestruction.Destruction(targetObject, shapeType, position, size);
            
            // Mesh의 정점 총 갯수
            int vertexCount = 0;
            int trianglesCount = 0;
            foreach (var o in objects)
            {
                var mesh = o.GetComponent<MeshFilter>().sharedMesh;
                vertexCount += mesh.vertices.Length;
                trianglesCount += mesh.triangles.Length;
            }
          
            var destructNetworkDataList = new List<DestructNetworkData>();
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[trianglesCount];

            int vertexCountSum = 0;
            int trianglesCountSum = 0;
            foreach (var o in objects)
            {
                var netObject = await Runner.SpawnAsync(emptyPrefab, o.transform.position, o.transform.rotation);
                var mesh = o.GetComponent<MeshFilter>().sharedMesh;
                var mat = o.GetComponent<MeshRenderer>().sharedMaterial;

                destructNetworkDataList.Add(new ()
                {
                    ID = netObject.Id,
                    ObjectName = o.name,
                    MeshName = mesh.name,
                    MaterialName = mat.name,
                    SubMeshCount = (ushort)mesh.subMeshCount,
                    VerticesCount = (ushort)mesh.vertices.Length,
                    NormalsCount = (ushort)mesh.normals.Length,
                    UVCount = (ushort)mesh.uv.Length,
                    TrianglesCount = (ushort)mesh.triangles.Length,
                    
                    ForceX = force.x,
                    ForceY = force.y,
                    ForceZ = force.z,
                });
                
                // 메쉬 정보 묶기
                mesh.vertices.CopyTo(vertices, vertexCountSum);
                mesh.normals.CopyTo(normals, vertexCountSum);
                mesh.uv.CopyTo(uvs, vertexCountSum);
                mesh.triangles.CopyTo(triangles, trianglesCountSum);

                vertexCountSum += mesh.vertexCount;
                trianglesCountSum += mesh.triangles.Length;
                
                Destroy(o);
            }
            
            foreach (var playerRef in Runner.ActivePlayers.ToArray())
            {
                var obj = await Runner.SpawnAsync(socketPrefab, Vector3.zero, Quaternion.identity, playerRef);
                var meshSocket = obj.GetComponent<NetworkMeshDestructSocket>();
                
                meshSocket.SetDestructData(destructNetworkDataList.ToArray());
                meshSocket.SetVerticesData(vertices);
                meshSocket.SetNormalsData(normals);
                meshSocket.SetUVsData(uvs);
                meshSocket.SetTrianglesData(triangles);
                
                meshSocket.SendStart();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void AddIsSuccessDestructionDictRPC(PlayerRef playerRef) => IsSuccessDestructionDict.Add(playerRef, true);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsSuccessDestructionDictRPC(PlayerRef playerRef, NetworkBool value) => IsSuccessDestructionDict.Set(playerRef, value);

        public struct DestructNetworkData : INetworkStruct
        {
            // public string ObjectName;
            // public int SubMeshCount;
            public NetworkId ID;
            public NetworkString<_128> ObjectName;
            public NetworkString<_128> MeshName;
            public NetworkString<_128> MaterialName;
            public ushort SubMeshCount;
            public ushort VerticesCount;
            public ushort NormalsCount;
            public ushort UVCount;
            public ushort TrianglesCount;

            public float ForceX;
            public float ForceY;
            public float ForceZ;
        }
    }
}