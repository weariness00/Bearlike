﻿using System;
using System.Collections;
using System.IO;
using Fusion;
using Manager;
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
        // 전송 받은 데이터 들 비트 단위로 계산
        [Networked] public UInt64 SendVerticesLayer { get; set; }
        [Networked] public UInt64 SendNormalsLayer { get; set; }
        [Networked] public UInt64 SendUVsLayer { get; set; }
        [Networked] public UInt64 SendTrianglesLayer { get; set; }
        
        // 전송 받아야할 패킷 갯수 255이 최대치
        [Networked] public byte VerticesCapacity { get; set; }
        [Networked] public byte NormalsCapacity { get; set; }
        [Networked] public byte UVsCapacity { get; set; }
        [Networked] public byte TrianglesCapacity { get; set; }
        
        // 패킷의 최종 크기
        [Networked] public int VerticesSize { get; set; }
        [Networked] public int NormalsSize { get; set; }
        [Networked] public int UVsSize { get; set; }
        [Networked] public int TrianglesSize { get; set; }

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
        private byte[] trianglesByte;

        // 전송항 데이터를 분할
        private byte[][] _verticesDivideByte;
        private byte[][] _normalsDivideByte;
        private byte[][] _uvsDivideByte;
        private byte[][] _trianglesDivideByte;

        // 전송이 성공했음에 대한 여부
        [Header("Is Send Complete")]
        [SerializeField] private bool _sendCompleteDestructData; 
        [SerializeField] private bool _sendCompleteVertices; 
        [SerializeField] private bool _sendCompleteNormals; 
        [SerializeField] private bool _sendCompleteUVs; 
        [SerializeField] private bool _sendCompleteTriangles;
        private bool _isDestruct;
        
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
            VerticesCapacity = (byte)(vByte.Length / NetworkMeshDestructSystem.SendByteCapacity + 1);
            _verticesDivideByte = new byte[VerticesCapacity][];
            for (int i = 0; i < _verticesDivideByte.Length; i++)
            {
                int capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == VerticesCapacity - 1) capacity = vByte.Length % capacity;
                _verticesDivideByte[i] = new byte[capacity];
                Array.Copy(vByte, i * NetworkMeshDestructSystem.SendByteCapacity, _verticesDivideByte[i], 0, capacity);
            }

            SendVerticesLayer = 0;
            VerticesSize = vByte.Length;
        }
        
        public void SetNormalsData(Vector3[] n)
        {
            byte[] nByte = SerializeVector3(n);
            NormalsCapacity = (byte)(nByte.Length / NetworkMeshDestructSystem.SendByteCapacity + 1);
            _normalsDivideByte = new byte[NormalsCapacity][];
            for (int i = 0; i < _normalsDivideByte.Length; i++)
            {
                int capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == NormalsCapacity - 1) capacity = nByte.Length % capacity;
                _normalsDivideByte[i] = new byte[capacity];
                Array.Copy(nByte, i * NetworkMeshDestructSystem.SendByteCapacity, _normalsDivideByte[i], 0, capacity);
            }
            
            SendNormalsLayer = 0;
            NormalsSize = nByte.Length;
        }
        
        public void SetUVsData(Vector2[] u)
        {
            byte[] uByte = SerializeVector2(u);
            UVsCapacity = (byte)(uByte.Length / NetworkMeshDestructSystem.SendByteCapacity + 1);
            _uvsDivideByte = new byte[UVsCapacity][];
            for (int i = 0; i < _uvsDivideByte.Length; i++)
            {
                int capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == UVsCapacity - 1) capacity = uByte.Length % capacity;
                _uvsDivideByte[i] = new byte[capacity];
                Array.Copy(uByte, i * NetworkMeshDestructSystem.SendByteCapacity, _uvsDivideByte[i], 0, capacity);
            }
            
            SendUVsLayer = 0;
            UVsSize = uByte.Length;
        }
        
        public void SetTrianglesData(int[] t)
        {
            byte[] tByte = SerializeInt(t);
            TrianglesCapacity = (byte)(tByte.Length / NetworkMeshDestructSystem.SendByteCapacity + 1);
            _trianglesDivideByte = new byte[TrianglesCapacity][];
            for (int i = 0; i < _trianglesDivideByte.Length; i++)
            {
                int capacity = NetworkMeshDestructSystem.SendByteCapacity;
                if (i == TrianglesCapacity - 1) capacity = tByte.Length % capacity;
                _trianglesDivideByte[i] = new byte[capacity];
                Array.Copy(tByte, i * NetworkMeshDestructSystem.SendByteCapacity, _trianglesDivideByte[i], 0, capacity);
            }
            
            SendTrianglesLayer = 0;
            TrianglesSize = tByte.Length;
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
                    if ((SendVerticesLayer & (1UL << i)) == 0)
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
                    StartCoroutine(SendVerticesCompleteCoroutine());
                    break;
                }
                yield return null;
            }
        }
        
        public IEnumerator SendNormalsCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < NormalsCapacity; i++)
                {
                    if ((SendNormalsLayer & (1UL << i)) == 0)
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
                    StartCoroutine(SendNormalsCompleteCoroutine());
                    break;
                }
                yield return null;
            }
        }

        public IEnumerator SendUVsCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < UVsCapacity; i++)
                {
                    if ((SendUVsLayer & (1UL << i)) == 0)
                    {
                        SendUVsRPC(i, _uvsDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendUVsLayer)
                {
                    StartCoroutine(SendUVsCompleteCoroutine());
                    break;
                }
                yield return null;
            }
        }
        
        public IEnumerator SendTrianglesCoroutine()
        {
            while (true)
            {
                UInt64 layer = 0;
                for (byte i = 0; i < TrianglesCapacity; i++)
                {
                    if ((SendTrianglesLayer & (1UL << i)) == 0)
                    {
                        SendTrianglesRPC(i, _trianglesDivideByte[i]);
                        yield return null;
                    }
                    else
                    {
                        layer |= 1UL << i;
                    }
                }

                if (layer == SendTrianglesLayer)
                {
                    StartCoroutine(SendTrianglesCompleteCoroutine());
                    break;
                }
                yield return null;
            }
        }
        
        #endregion

        // 모든 데이터를 전송했다는 것을 알리는 RPC가 도중에 자꾸 손실되어 Coroutine으로 만듬
        #region Complete Coroutine

        private IEnumerator SendVerticesCompleteCoroutine()
        {
            while (true)
            {
                SetSendVerticesCompleteRPC(true);
                yield return null;
            }
        }
        
        private IEnumerator SendNormalsCompleteCoroutine()
        {
            while (true)
            {
                SetSendNormalsCompleteRPC(true);
                yield return null;
            }
        }
        
        private IEnumerator SendUVsCompleteCoroutine()
        {
            while (true)
            {
                SetSendUVsCompleteRPC(true);
                yield return null;
            }
        }
        
        private IEnumerator SendTrianglesCompleteCoroutine()
        {
            while (true)
            {
                SetSendTrianglesCompleteRPC(true);
                yield return null;
            }
        }
        
        #endregion

        #region NetworkData Setting Coroutine
        
        IEnumerator SetHasVerticesDataCoroutine(UInt64 value)
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

        IEnumerator DestroyCoroutine()
        {
            while (gameObject != null)
            {
                DestroyRPC();
                yield return null;
            }
        }
        
        private void Destruct()
        {
            if(_isDestruct) return;
            if (!_sendCompleteDestructData || !_sendCompleteVertices || !_sendCompleteNormals || !_sendCompleteUVs || !_sendCompleteTriangles) return;

            _isDestruct = true;

            uint vertexCount = 0;
            uint normalCount = 0;
            uint uvCount = 0;
            uint triangleCount = 0;

            Vector3[] vertices = DeserializeVector3(verticesByte);
            Vector3[] normals = DeserializeVector3(normalsByte);
            Vector2[] uvs = DeserializeVector2(uvsByte);
            int[] triangles = DeserializeInt(trianglesByte);
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
                var msehRenderer = netObject.GetComponent<MeshRenderer>();
                var meshCollider = netObject.gameObject.AddComponent<MeshCollider>();

                meshFilter.sharedMesh = mesh;
                
                msehRenderer.material = NetworkMeshDestructObject.GetMaterial((string)data.MaterialName);
                if (netObject.name.Contains("_Slicing") || netObject.name.Contains("_Intersect"))
                {
                    meshCollider.convex = true;
                    var rb = netObject.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.AddForce(new Vector3(data.ForceX, data.ForceY, data.ForceZ).normalized * 100);
                }
                meshCollider.sharedMesh = mesh;
            }
            
            NetworkMeshDestructSystem.Instance.SetIsSuccessDestructionDict(Runner.LocalPlayer, true);
            DebugManager.Log($"{Runner.LocalPlayer}의 객체 붕괴 성공");

            StartCoroutine(DestroyCoroutine());
        }

        #endregion

        #region 직렬화/역 직렬화

        // 직렬화
        private byte[] SerializeInt(int[] array)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((short)array.Length);
                    foreach (var i in array)
                    {
                        binaryWriter.Write((short)i);
                    }
                }
                return memoryStream.ToArray();
            }
        }
        
        private byte[] SerializeVector3(Vector3[] vectors)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((short)vectors.Length);
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
                    binaryWriter.Write((short)vectors.Length);
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
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // 배열 길이를 먼저 읽습니다.
                    int length = binaryReader.ReadInt16();
                    int[] array = new int[length];
            
                    // 각 Vector3의 x, y, z 값을 순차적으로 읽습니다.
                    for (int i = 0; i < length; i++)
                    {
                        array[i] = binaryReader.ReadInt16();
                    }
            
                    return array;
                }
            }
        }
        
        private Vector3[] DeserializeVector3(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // 배열 길이를 먼저 읽습니다.
                    int length = binaryReader.ReadInt16();
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
                    int length = binaryReader.ReadInt16();
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
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void SetHasVerticesRPC(UInt64 value) => SendVerticesLayer = value;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void SetHasNormalsRPC(UInt64 value) => SendNormalsLayer = value;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void SetHasUVsRPC(UInt64 value) => SendUVsLayer = value;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void SetHasTrianglesRPC(UInt64 value) => SendTrianglesLayer = value;

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        public void SetSendVerticesCompleteRPC(NetworkBool value)
        {
            _sendCompleteVertices = value;
            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        public void SetSendNormalsCompleteRPC(NetworkBool value)
        {
            _sendCompleteNormals = value;
            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        public void SetSendUVsCompleteRPC(NetworkBool value)
        {
            _sendCompleteUVs = value;
            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        public void SetSendTrianglesCompleteRPC(NetworkBool value)
        {
            _sendCompleteTriangles = value;
            Destruct();
        }
        
        #endregion

        #region Send Fucntion
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void SendDestructDataRPC(NetworkMeshDestructSystem.DestructNetworkData[] data)
        {
            destructNetworkData = data;
            _sendCompleteDestructData = true;
            Destruct();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Unreliable)]
        public void SendVerticesRPC(byte number, byte[] data)
        {
            verticesByte ??= new byte[VerticesSize];
            Array.Copy(data, 0, verticesByte, NetworkMeshDestructSystem.SendByteCapacity * number, data.Length);

            _hasVertices |= (1UL << number);
            if(_setHasVerticesCoroutine != null) StopCoroutine(_setHasVerticesCoroutine);
            _setHasVerticesCoroutine = StartCoroutine(SetHasVerticesDataCoroutine(_hasVertices));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Unreliable)]
        public void SendNormalsRPC(byte number, byte[] data)
        {
            normalsByte ??= new byte[NormalsSize];
            Array.Copy(data, 0, normalsByte, NetworkMeshDestructSystem.SendByteCapacity * number, data.Length);

            _hasNormals |= (1UL << number);
            if(_setHasNormalsCoroutine != null) StopCoroutine(_setHasNormalsCoroutine);
            _setHasNormalsCoroutine = StartCoroutine(SetHasNormalsDataCoroutine(_hasNormals));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Unreliable)]
        public void SendUVsRPC(byte number, byte[] data)
        {
            uvsByte ??= new byte[UVsSize];
            Array.Copy(data, 0, uvsByte, NetworkMeshDestructSystem.SendByteCapacity * number, data.Length);

            _hasUVs |= (1UL << number);
            if(_setHasUVsCoroutine != null) StopCoroutine(_setHasUVsCoroutine);
            _setHasUVsCoroutine = StartCoroutine(SetHasUVsDataCoroutine(_hasUVs));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Unreliable)]
        public void SendTrianglesRPC(byte number, byte[] data)
        {
            trianglesByte ??= new byte[TrianglesSize];
            Array.Copy(data, 0, trianglesByte, NetworkMeshDestructSystem.SendByteCapacity * number, data.Length);

            _hasTriangles |= (1UL << number);
            if(_setHasTrianglesCoroutine != null) StopCoroutine(_setHasTrianglesCoroutine);
            _setHasTrianglesCoroutine = StartCoroutine(SetHasTrianglesDataCoroutine(_hasTriangles));
        }
        
        #endregion

        #endregion
    }
}