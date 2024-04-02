using System;
using System.Collections;
using System.IO;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace Photon
{
    /// <summary>
    /// 메쉬 정보를 송수신하는 용도로 사용하는 socket 객체
    /// 
    /// </summary>
    public class NetworkMeshDestructSocket : NetworkBehaviourEx
    {
        [Networked] public UInt64 SendVerticesLayer { get; set; }
        [Networked] public UInt64 SendNormalsLayer { get; set; }
        [Networked] public UInt64 SendUVsLayer { get; set; }
        [Networked] public UInt64 SendTrianglesLayer { get; set; }
        [Networked] public byte VerticesCapacity { get; set; }
        [Networked] public byte NormalsCapacity { get; set; }
        [Networked] public byte UVsCapacity { get; set; }
        [Networked] public byte TrianglesCapacity { get; set; }

        // RPC는 한번에 1개밖에 실행이 안됨으로 코루틴을 돌려 실시간 업데이트가 되도록 실행
        private Coroutine _setHasVerticesCoroutine;
        private Coroutine _setHasNormalsCoroutine;
        private Coroutine _setHasUVsCoroutine;
        private Coroutine _setHasTrianglesCoroutine;
        
        // Layer 개념으로 사용될 것
        // 분할 전송된 데이터는 numbering을 해서 보내기에 총 64개까지 분할하여 전송이 가능 => UInt64는 총 64비트이기떄문
        private UInt64 _hasVertices;
        private UInt64 _hasNormals;
        private UInt64 _hasUVs;
        private UInt64 _hasTriangles;
        
        // Input 권한이 있는 클라이언트가 실제 전송받는 데이터
        private NetworkMeshDestructSystem.DestructNetworkData[] destructNetworkData;
        private byte[] verticesByte;
        private byte[] normalsByte;
        private byte[] uvsByte;
        private byte[] triangles;

        // 전송항 데이터를 분할
        private byte[][] _verticesDivideByte;
        private byte[][] _normalsDivideByte;
        private byte[][] _uvsDivideByte;
        private byte[][] _trianglesDivideByte;

        private bool _sendCompleteDestructData; 
        private bool _sendCompleteVertices; 
        private bool _sendCompleteNormals; 
        private bool _sendCompleteUVs; 
        private bool _sendCompleteTriangles; 

        #region Member Function

        public void SendStart()
        {
            SendDestructDataRPC(destructNetworkData);
            StartCoroutine(SendVerticesCoroutine());
            StartCoroutine(SendNormalsCoroutine());
            StartCoroutine(SendUVsCoroutine());
            StartCoroutine(SendTrianglesCoroutine());
        }

        #region Data Setting
        
        public void SetDestructData(NetworkMeshDestructSystem.DestructNetworkData[] data)
        {
            destructNetworkData = data;
        }

        public void SetVerticesData(Vector3[] v)
        {
            byte[] vByte = SerializeVector3(v);
            _verticesDivideByte = new byte[vByte.Length / NetworkMeshDestructSystem.SendByteCapacity][];
            for (int i = 0; i < _verticesDivideByte.Length; i++)
            {
                uint capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == VerticesCapacity - 1) capacity = (uint)vByte.Length % capacity;
                byte[] data = new byte[capacity];
                _verticesDivideByte[i] = new byte[capacity];
                Array.Copy(_verticesDivideByte[i], 0, vByte, i * NetworkMeshDestructSystem.SendByteCapacity, i * NetworkMeshDestructSystem.SendByteCapacity + capacity);
            }
            vByte = null;
        }
        
        public void SetNormalsData(Vector3[] n)
        {
            byte[] nByte = SerializeVector3(n);
            _normalsDivideByte = new byte[nByte.Length / NetworkMeshDestructSystem.SendByteCapacity][];
            for (int i = 0; i < _normalsDivideByte.Length; i++)
            {
                uint capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == NormalsCapacity - 1) capacity = (uint)nByte.Length % capacity;
                byte[] data = new byte[capacity];
                _normalsDivideByte[i] = new byte[capacity];
                Array.Copy(_normalsDivideByte[i], 0, nByte, i * NetworkMeshDestructSystem.SendByteCapacity, i * NetworkMeshDestructSystem.SendByteCapacity + capacity);
            }
            nByte = null;
        }
        
        public void SetUVsData(Vector2[] u)
        {
            byte[] uByte = SerializeVector2(u);
            _uvsDivideByte = new byte[uByte.Length / NetworkMeshDestructSystem.SendByteCapacity][];
            for (int i = 0; i < _uvsDivideByte.Length; i++)
            {
                uint capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == UVsCapacity - 1) capacity = (uint)uByte.Length % capacity;
                byte[] data = new byte[capacity];
                _uvsDivideByte[i] = new byte[capacity];
                Array.Copy(_uvsDivideByte[i], 0, uByte, i * NetworkMeshDestructSystem.SendByteCapacity, i * NetworkMeshDestructSystem.SendByteCapacity + capacity);
            }
            uByte = null;
        }
        
        public void SetTrianglesData(int[] t)
        {
            byte[] tByte = SerializeInt(t);
            _trianglesDivideByte = new byte[tByte.Length / NetworkMeshDestructSystem.SendByteCapacity][];
            for (int i = 0; i < _trianglesDivideByte.Length; i++)
            {
                uint capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == UVsCapacity - 1) capacity = (uint)tByte.Length % capacity;
                byte[] data = new byte[capacity];
                _trianglesDivideByte[i] = new byte[capacity];
                Array.Copy(_trianglesDivideByte[i], 0, tByte, i * NetworkMeshDestructSystem.SendByteCapacity, i * NetworkMeshDestructSystem.SendByteCapacity + capacity);
            }
            tByte = null;
        }
        
        #endregion

        #region Data Send

        public IEnumerator SendVerticesCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < VerticesCapacity; i++)
                {
                    if ((SendVerticesLayer & (1UL << i)) != 0)
                    {
                        SendVerticesRPC(i, _verticesDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendVerticesLayer)
                {
                    break;
                }
            }
        }
        
        public IEnumerator SendNormalsCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < NormalsCapacity; i++)
                {
                    if ((SendNormalsLayer & (1UL << i)) != 0)
                    {
                        SendNormalsRPC(i, _normalsDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendNormalsLayer)
                {
                    break;
                }
            }
        }

        public IEnumerator SendUVsCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < UVsCapacity; i++)
                {
                    if ((SendUVsLayer & (1UL << i)) != 0)
                    {
                        SendNormalsRPC(i, _uvsDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendUVsLayer)
                {
                    break;
                }
            }
        }
        
        public IEnumerator SendTrianglesCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < TrianglesCapacity; i++)
                {
                    if ((SendTrianglesLayer & (1UL << i)) != 0)
                    {
                        SendNormalsRPC(i, _trianglesDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendTrianglesLayer)
                {
                    break;
                }
            }
        }
        
        #endregion

        #region NetworkData Setting Coroutine
        
        IEnumerator SetHasVerticesDataDictCoroutine(UInt64 value)
        {
            while (SendVerticesLayer != value)
            {
                SetHasVerticesRPC(value);
                yield return null;
            }
        }
        
        IEnumerator SetHasNormalsDataCoroutine(UInt64 value)
        {
            while (SendNormalsLayer != value)
            {
                SetHasNormalsRPC(value);
                yield return null;
            }
        }
        
        IEnumerator SetHasUVsDataCoroutine(UInt64 value)
        {
            while (SendUVsLayer != value)
            {
                SetHasUVsRPC(value);
                yield return null;
            }
        }
        IEnumerator SetHasTrianglesDataCoroutine(UInt64 value)
        {
            while (SendTrianglesLayer != value)
            {
                SetHasTrianglesRPC(value);
                yield return null;
            }
        }
        
        #endregion
        
        private void Destruct()
        {
            if (_sendCompleteDestructData && _sendCompleteVertices && _sendCompleteNormals && _sendCompleteUVs && _sendCompleteTriangles)
            {
                return;
            }

            ushort vertexCount = 0;
            ushort normalCount = 0;
            ushort uvCount = 0;
            ushort triangleCount = 0;

            Vector3[] vertices = DeserializeVector3(verticesByte);
            Vector3[] normals = DeserializeVector3(normalsByte);
            Vector2[] uvs = DeserializeVector2(uvsByte);
            foreach (var data in destructNetworkData)
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
                
                Array.Copy(vertices, vertexCount, v, 0, data.VerticesCount);
                Array.Copy(normals, normalCount, n, 0, data.NormalsCount);
                Array.Copy(uvs, uvCount, u, 0, data.UVCount);
                Array.Copy(triangles, triangleCount, t, 0, data.TrianglesCount);

                mesh.SetVertices(v);
                mesh.SetNormals(n);
                mesh.SetUVs(0, u);
                mesh.SetTriangles(t, 0);
                
                vertexCount += data.VerticesCount;
                normalCount += data.NormalsCount;
                uvCount += data.UVCount;
                triangleCount += data.TrianglesCount;
            
                // 컴포넌트 초기화
                var meshFilter = netObject.GetComponent<MeshFilter>();
                var renderer = netObject.GetComponent<MeshRenderer>();
                var meshCollider = netObject.gameObject.AddComponent<MeshCollider>();

                meshFilter.sharedMesh = mesh;
                
                renderer.material = NetworkMeshDestructObject.GetMaterial((string)data.MaterialName);
                if (netObject.name.Contains("_Slicing") || netObject.name.Contains("_Intersect"))
                {
                    meshCollider.convex = true;
                    var rb = netObject.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.AddForce(new Vector3(data.ForceX, data.ForceY, data.ForceZ));
                }
                meshCollider.sharedMesh = mesh;
            }
            
            StartCoroutine(NetworkMeshDestructSystem.Instance.IsSuccessDestructionDictCoroutine(Runner.LocalPlayer, true));
            DestroyRPC();
        }

        #endregion

        #region 직렬화/역 직렬화

        // 직렬화
        private byte[] SerializeInt(int[] array)
        {
            byte[] byteArray = new byte[array.Length * sizeof(int)];
            for (int i = 0; i < array.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(array[i]);
                Array.Copy(temp, 0, byteArray, i * sizeof(int), sizeof(int));
            }
            return byteArray;
        }
        
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
        
        // 역 직렬화
        private int[] DeserializeInt(byte[] bytes)
        {
            int[] intArray = new int[bytes.Length / sizeof(int)];
            for (int i = 0; i < intArray.Length; i++)
            {
                intArray[i] = BitConverter.ToInt32(bytes, i * sizeof(int));
            }
            return intArray;
        }
        
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

        #region RPC Function

        #region Set Function
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetHasVerticesRPC(UInt64 value) => SendVerticesLayer = value;
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetHasNormalsRPC(UInt64 value) => SendNormalsLayer = value;
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetHasUVsRPC(UInt64 value) => SendUVsLayer = value;
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetHasTrianglesRPC(UInt64 value) => SendTrianglesLayer = value;
        
        #endregion

        #region Send Fucntion
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendDestructDataRPC(NetworkMeshDestructSystem.DestructNetworkData[] data)
        {
            destructNetworkData = data;
            _sendCompleteDestructData = true;
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendVerticesRPC(byte number, byte[] data)
        {
            Array.Copy(verticesByte, NetworkMeshDestructSystem.SendByteCapacity * number, data, 0, data.Length);

            _hasVertices |= (uint)(1 << number);
            if(_setHasVerticesCoroutine != null) StopCoroutine(_setHasVerticesCoroutine);
            _setHasVerticesCoroutine = StartCoroutine(SetHasVerticesDataDictCoroutine(_hasVertices));

            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendNormalsRPC(byte number, byte[] data)
        {
            Array.Copy(normalsByte, NetworkMeshDestructSystem.SendByteCapacity * number, data, 0, data.Length);

            _hasNormals |= (uint)(1 << number);
            if(_setHasNormalsCoroutine != null) StopCoroutine(_setHasNormalsCoroutine);
            _setHasNormalsCoroutine = StartCoroutine(SetHasNormalsDataCoroutine(_hasNormals));

            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendUVsRPC(byte number, byte[] data)
        {
            Array.Copy(uvsByte, NetworkMeshDestructSystem.SendByteCapacity * number, data, 0, data.Length);

            _hasUVs |= (uint)(1 << number);
            if(_setHasUVsCoroutine != null) StopCoroutine(_setHasUVsCoroutine);
            _setHasUVsCoroutine = StartCoroutine(SetHasUVsDataCoroutine(_hasUVs));

            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendTrianglesRPC(byte number, byte[] data)
        {
            Array.Copy(triangles, NetworkMeshDestructSystem.SendByteCapacity * number, data, 0, data.Length);

            _hasTriangles |= (uint)(1 << number);
            if(_setHasTrianglesCoroutine != null) StopCoroutine(_setHasTrianglesCoroutine);
            _setHasTrianglesCoroutine = StartCoroutine(SetHasTrianglesDataCoroutine(_hasTriangles));

            Destruct();
        }
        
        #endregion

        #endregion
    }
}