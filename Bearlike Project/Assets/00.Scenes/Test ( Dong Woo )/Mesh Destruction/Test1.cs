using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Test
{
    public class TestSliceComputeShader : MonoBehaviour
    {
        public ComputeShader sliceShader;
        public ComputeShader polygonShader;
        
        private struct CSParam
        {
            public const string MeshDataConvertKernel = "MeshDataConvert";
            public const int ThreadX = 32;
            
            public static readonly int Vertices = Shader.PropertyToID("vertices");
            public static readonly int Normals = Shader.PropertyToID("normals");
            public static readonly int UVs = Shader.PropertyToID("uvs");
            public static readonly int Triangles = Shader.PropertyToID("triangles");
            public static readonly int TriangleLength = Shader.PropertyToID("triangleLength");
            public static readonly int OutputPolygonData = Shader.PropertyToID("outputPolygonData");
        }
        
        #region Unity Enven Function

        private void Awake()
        {
            Slice(gameObject,Vector3.up, transform.position);
        }

        #endregion
        
        #region Data Structure
        /// <summary>
        /// 쪼개진 mesh 정보들을 담을 클래스
        /// </summary>
        public class SliceInfo
        {
            public List<MeshDotData> DotList = new List<MeshDotData>();
            public List<int> Triangles = new List<int>();

            public SliceInfo(params SliceInfo[] infos)
            {
                foreach (var sliceInfo in infos)
                {
                    this.AddRange(sliceInfo);
                }
            }

            public void AddRange(SliceInfo other)
            {
                // 폴리곤의 버텍스 인덱스 재정의
                for (int i = 0; i < other.Triangles.Count; i++)
                {
                    other.Triangles[i] += DotList.Count;
                }
                DotList.AddRange(other.DotList);
                Triangles.AddRange(other.Triangles);
            }
        }

        /// <summary>
        /// 폴리곤 데이터를 담을 구조체
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct PolygonData
        {
            public MeshDotData Dot0;
            public MeshDotData Dot1;
            public MeshDotData Dot2;
        }

        /// <summary>
        /// Mesh의 한 점의 정보를 담는 자료형
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MeshDotData
        {
            public Vector3 Vertex; // 12 bytes
            public float Pad0;     // 4 bytes to align to 16 bytes

            public Vector3 Normal; // 12 bytes
            public float Pad1;     // 4 bytes to align to 16 bytes

            public Vector2 UV;     // 8 bytes
            public int Index;      // 4 bytes
            public float Pad2;     // 추가적인 4바이트 패딩
        }
        
        #endregion

        public void Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint)
        {
            Mesh mesh = targetObject.GetComponent<MeshFilter>().sharedMesh;
            int dotCount = mesh.vertices.Length;
            int triangleCount = mesh.triangles.Length;
            int polygonCount = triangleCount / 3;
            ComputeBuffer verticesBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer normalsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer uvsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 2);
            ComputeBuffer trianglesBuffer = new ComputeBuffer(triangleCount, sizeof(int));
            ComputeBuffer polygonsBuffer = new ComputeBuffer(polygonCount, Marshal.SizeOf(typeof(PolygonData)));
            PolygonData[] polygonData = new PolygonData[polygonCount];
            
            verticesBuffer.SetData(mesh.vertices);
            normalsBuffer.SetData(mesh.normals);
            uvsBuffer.SetData(mesh.uv);
            trianglesBuffer.SetData(mesh.triangles);
            
            int kernelID = polygonShader.FindKernel(CSParam.MeshDataConvertKernel);
            polygonShader.SetInt(CSParam.TriangleLength, polygonCount);
            polygonShader.SetBuffer(kernelID, CSParam.Vertices, verticesBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.Normals, normalsBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.UVs, uvsBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.Triangles, trianglesBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.OutputPolygonData, polygonsBuffer);

            var threadX = polygonCount / CSParam.ThreadX;
            polygonShader.Dispatch(kernelID, threadX + 1,1,1);
            
            polygonsBuffer.GetData(polygonData);
            
            verticesBuffer.Release();
            normalsBuffer.Release();
            uvsBuffer.Release();
            polygonsBuffer.Release();
        }
    }
}

