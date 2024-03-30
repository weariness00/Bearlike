using System.IO;
using Fusion;
using Manager;
using UnityEngine;
using Util;

namespace Photon
{
    public class NetworkMeshDestructSystem : NetworkSingleton<NetworkMeshDestructSystem>
    {
        public NetworkPrefabRef emptyPrefab;
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void DestructRPC(NetworkId id, PrimitiveType shapeType, Vector3 position, Vector3 size, Vector3 force)
        {
            var targetObject = Runner.FindObject(id).gameObject;
            var objects = MeshDestruction.Destruction(targetObject, shapeType, position, size, force);
            foreach (var o in objects)
            {
                var netObject = await Runner.SpawnAsync(emptyPrefab, o.transform.position, o.transform.rotation);
                var mesh = o.GetComponent<MeshFilter>().sharedMesh;
                SetMeshRPC(netObject.Id, o.name,
                    mesh.name, mesh.subMeshCount,
                    SerializeVector3(mesh.vertices), 
                    SerializeVector3(mesh.normals), 
                    SerializeVector2(mesh.uv), 
                    mesh.triangles);
                
                Destroy(o);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetMeshRPC(NetworkId id, string objectName ,string meshName, int subMeshCount, byte[] vertices, byte[] normals, byte[] uv, int[] triangles)
        {
            var meshObject = Runner.FindObject(id);
            meshObject.name = objectName;
            Mesh mesh = new Mesh
            {
                subMeshCount = subMeshCount,
                name = meshName,
                vertices = DeserializeVector3(vertices),
                normals = DeserializeVector3(normals),
                uv = DeserializeVector2(uv)
            };
            mesh.SetTriangles(triangles, 0);
            
            var meshCollider = meshObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            
            DebugManager.ToDo("네트워크 상에서 메쉬 붕괴를 하면 Meterial 정보도 받아오게 해야됨");
        }

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
    }
}