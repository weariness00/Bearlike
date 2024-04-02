using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fusion;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using Util;

namespace Photon
{
    public class NetworkMeshDestructSystem : NetworkSingleton<NetworkMeshDestructSystem>
    {
        [Networked, Capacity(3)] public NetworkDictionary<PlayerRef, NetworkBool> IsSuccessDestructionDict { get; }
        public NetworkPrefabRef emptyPrefab;

        private DestructNetworkData[] _destructDatas = Array.Empty<DestructNetworkData>();
        private Vector3[] _vertices= Array.Empty<Vector3>();
        private Vector3[] _normals= Array.Empty<Vector3>();
        private Vector2[] _uvs= Array.Empty<Vector2>();
        private int[] _triangles= Array.Empty<int>();

        public override void Spawned()
        {
            AddIsSuccessDestructionDictRPC(Runner.LocalPlayer);
        }

        private void Destruct()
        {
            if (_destructDatas.Length == 0 || _vertices.Length == 0 || _normals.Length == 0 || _uvs.Length == 0 || _triangles.Length == 0)
            {
                return;
            }

            ushort vertexCount = 0;
            ushort normalCount = 0;
            ushort uvCount = 0;
            ushort triangleCount = 0;
            foreach (var data in _destructDatas)
            {
                var netObject = Runner.FindObject(data.ID);
                netObject.name = (string)data.ObjectName;
                Mesh mesh = new Mesh
                {
                    name = (string)data.MeshName,
                    subMeshCount = data.SubMeshCount,
                };
                // Mesh 정보 저장
                Vector3[] v = new Vector3[data.VerticesCount];
                Vector3[] n = new Vector3[data.NormalsCount];
                Vector2[] u = new Vector2[data.UVCount];
                int[] t= new int[data.TrianglesCount];
                
                Array.Copy(_vertices, vertexCount, v, 0, data.VerticesCount);
                Array.Copy(_normals, normalCount, n, 0, data.NormalsCount);
                Array.Copy(_uvs, uvCount, u, 0, data.UVCount);
                Array.Copy(_triangles, triangleCount, t, 0, data.TrianglesCount);

                mesh.SetVertices(v);
                mesh.SetNormals(n);
                mesh.SetUVs(0, u);
                mesh.SetTriangles(t, 0);
                
                vertexCount += data.VerticesCount;
                normalCount += data.NormalsCount;
                uvCount += data.UVCount;
                triangleCount += data.TrianglesCount;
            
                // 컴포넌트 초기화
                netObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                var meshCollider = netObject.gameObject.AddComponent<MeshCollider>();
                if (netObject.name.Contains("_Slice"))
                {
                    var rb = netObject.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                }
                meshCollider.convex = true;
                meshCollider.sharedMesh = mesh;
            
                DebugManager.ToDo("네트워크 상에서 메쉬 붕괴를 하면 Meterial 정보도 받아오게 해야됨");
            }
            
            _destructDatas = Array.Empty<DestructNetworkData>();
            _vertices= Array.Empty<Vector3>();
            _normals= Array.Empty<Vector3>();
            _uvs= Array.Empty<Vector2>();
            _triangles= Array.Empty<int>();
            
            SetIsSuccessDestructionDictRPC(Runner.LocalPlayer, true);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SendDestructDataRPC(DestructNetworkData[] datas)
        {
            _destructDatas = datas;
            Destruct();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SendVerticesRPC(byte[] bytes)
        {
            _vertices = DeserializeVector3(bytes);
            Destruct();
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SendNormalsRPC(byte[] bytes)
        {
            _normals = DeserializeVector3(bytes);
            Destruct();
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SendUVsRPC(byte[] bytes)
        {
            _uvs = DeserializeVector2(bytes);
            Destruct();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SendTrianglesRPC(int[] triangles)
        {
            _triangles = triangles;
            Destruct();
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
            var objects = MeshDestruction.Destruction(targetObject, shapeType, position, size, force);
            
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

                destructNetworkDataList.Add(new ()
                {
                    ID = netObject.Id,
                    ObjectName = o.name,
                    MeshName = mesh.name,
                    SubMeshCount = (ushort)mesh.subMeshCount,
                    VerticesCount = (ushort)mesh.vertices.Length,
                    NormalsCount = (ushort)mesh.normals.Length,
                    UVCount = (ushort)mesh.uv.Length,
                    TrianglesCount = (ushort)mesh.triangles.Length,
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
            
            // 묶은 메쉬 정보 전송
            SendDestructDataRPC(destructNetworkDataList.ToArray());
            SendVerticesRPC(SerializeVector3(vertices));
            SendNormalsRPC(SerializeVector3(normals));
            SendUVsRPC(SerializeVector2(uvs));
            SendTrianglesRPC(triangles);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void AddIsSuccessDestructionDictRPC(PlayerRef playerRef) => IsSuccessDestructionDict.Add(playerRef, true);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsSuccessDestructionDictRPC(PlayerRef playerRef, NetworkBool value) => IsSuccessDestructionDict.Set(playerRef, value);
        
        #region Vector 직렬화/역직렬화
        
        private byte[] SerializeVector3(Vector3[] vectors)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(vectors.Length);
                    foreach (var vector in vectors)
                    {
                        binaryWriter.Write(vector.x);
                        binaryWriter.Write(vector.y);
                        binaryWriter.Write(vector.z);
                    }
                }
                return memoryStream.ToArray();
            }
        }

        // 바이트 배열을 Vector3로 역직렬화
        private Vector3[] DeserializeVector3(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // 배열 길이를 먼저 읽습니다.
                    int length = binaryReader.ReadInt32();
                    Vector3[] vectors = new Vector3[length];
            
                    // 각 Vector3의 x, y, z 값을 순차적으로 읽습니다.
                    for (int i = 0; i < length; i++)
                    {
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        float z = binaryReader.ReadSingle();
                        vectors[i] = new Vector3(x, y, z);
                    }
            
                    return vectors;
                }
            }
        }
        
        private byte[] SerializeVector2(Vector2[] vectors)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(vectors.Length);
                    foreach (var vector in vectors)
                    {
                        binaryWriter.Write(vector.x);
                        binaryWriter.Write(vector.y);
                    }
                }
                return memoryStream.ToArray();
            }
        }

        // 바이트 배열을 Vector3로 역직렬화
        private Vector2[] DeserializeVector2(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // 배열 길이를 먼저 읽습니다.
                    int length = binaryReader.ReadInt32();
                    Vector2[] vectors = new Vector2[length];
            
                    // 각 Vector3의 x, y, z 값을 순차적으로 읽습니다.
                    for (int i = 0; i < length; i++)
                    {
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        vectors[i] = new Vector2(x, y);
                    }
            
                    return vectors;
                }
            }
        }
        
        #endregion

        public struct DestructNetworkData : INetworkStruct
        {
            // public string ObjectName;
            // public int SubMeshCount;
            public NetworkId ID;
            public NetworkString<_128> ObjectName;
            public NetworkString<_128> MeshName;
            public ushort SubMeshCount;
            public ushort VerticesCount;
            public ushort NormalsCount;
            public ushort UVCount;
            public ushort TrianglesCount;
        }
    }
}