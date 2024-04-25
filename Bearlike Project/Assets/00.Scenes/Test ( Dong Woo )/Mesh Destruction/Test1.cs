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
            
            public static readonly int SlicePoint = Shader.PropertyToID("slicePoint");
            public static readonly int SliceNormal = Shader.PropertyToID("sliceNormal");
            
            public static readonly int Vertices = Shader.PropertyToID("vertices");
            public static readonly int Normals = Shader.PropertyToID("normals");
            public static readonly int UVs = Shader.PropertyToID("uvs");
            public static readonly int Triangles = Shader.PropertyToID("triangles");
            public static readonly int PolygonLength = Shader.PropertyToID("polygonLength");
            public static readonly int DotLength = Shader.PropertyToID("dotLength");
            
            public static readonly int SliceData0 = Shader.PropertyToID("sliceData0");
            public static readonly int SliceData1 = Shader.PropertyToID("sliceData1");
            public static readonly int SliceCount0 = Shader.PropertyToID("sliceCount0");
            public static readonly int SliceCount1 = Shader.PropertyToID("sliceCount1");

            public static readonly int NewDotData = Shader.PropertyToID("newDotData");
            public static readonly int NewDotCount = Shader.PropertyToID("newDotCount");
        }
        
        #region Unity Enven Function

        private void Awake()
        {
            Slice(gameObject,Vector3.up, transform.position);
        }

        #endregion
        
        #region Data Structure

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
            slicePoint = targetObject.transform.position - slicePoint;
            
            int dotCount = mesh.vertices.Length;
            int triangleCount = mesh.triangles.Length;
            int polygonCount = triangleCount / 3;
            ComputeBuffer verticesBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer normalsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer uvsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 2);
            ComputeBuffer trianglesBuffer = new ComputeBuffer(triangleCount, sizeof(int));
            ComputeBuffer dotLengthBuffer = new ComputeBuffer(1, sizeof(uint));
            
            ComputeBuffer sliceDataBuffer0 = new ComputeBuffer(polygonCount, Marshal.SizeOf(typeof(PolygonData)));
            ComputeBuffer sliceDataBuffer1 = new ComputeBuffer(polygonCount, Marshal.SizeOf(typeof(PolygonData)));
            ComputeBuffer newDotDataBuffer = new ComputeBuffer(polygonCount * 2, Marshal.SizeOf(typeof(MeshDotData)));
            ComputeBuffer sliceCountBuffer0 = new ComputeBuffer(1, sizeof(uint));
            ComputeBuffer sliceCountBuffer1 = new ComputeBuffer(1, sizeof(uint));
            ComputeBuffer newDotCountBuffer = new ComputeBuffer(1, sizeof(uint));
                
            PolygonData[] slicePolygonData0 = new PolygonData[polygonCount];
            PolygonData[] slicePolygonData1 = new PolygonData[polygonCount];
            MeshDotData[] newDotData = new MeshDotData[polygonCount * 2];
            uint[] sliceCount0 = new uint[1];
            uint[] sliceCount1 = new uint[1];
            uint[] newDotCount = new uint[1];
            uint[] dotLength = new uint[]{(uint)dotCount};
            
            verticesBuffer.SetData(mesh.vertices);
            normalsBuffer.SetData(mesh.normals);
            uvsBuffer.SetData(mesh.uv);
            trianglesBuffer.SetData(mesh.triangles);
            dotLengthBuffer.SetData(dotLength);
            
            sliceDataBuffer0.SetData(slicePolygonData0);
            sliceDataBuffer1.SetData(slicePolygonData1);
            newDotDataBuffer.SetData(newDotData);
            sliceCountBuffer0.SetData(sliceCount0);
            sliceCountBuffer1.SetData(sliceCount1);
            newDotCountBuffer.SetData(newDotCount);
            
            int kernelID = polygonShader.FindKernel(CSParam.MeshDataConvertKernel);
            polygonShader.SetInt(CSParam.PolygonLength, polygonCount);
            polygonShader.SetVector(CSParam.SlicePoint, slicePoint);
            polygonShader.SetVector(CSParam.SliceNormal, sliceNormal);
            polygonShader.SetBuffer(kernelID, CSParam.Vertices, verticesBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.Normals, normalsBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.UVs, uvsBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.Triangles, trianglesBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.DotLength, dotLengthBuffer);
            
            polygonShader.SetBuffer(kernelID, CSParam.SliceData0, sliceDataBuffer0);
            polygonShader.SetBuffer(kernelID, CSParam.SliceData1, sliceDataBuffer1);
            polygonShader.SetBuffer(kernelID, CSParam.NewDotData, newDotDataBuffer);
            polygonShader.SetBuffer(kernelID, CSParam.SliceCount0, sliceCountBuffer0);
            polygonShader.SetBuffer(kernelID, CSParam.SliceCount1, sliceCountBuffer1);
            polygonShader.SetBuffer(kernelID, CSParam.NewDotCount, newDotCountBuffer);

            var threadX = polygonCount / CSParam.ThreadX;
            polygonShader.Dispatch(kernelID, threadX + 1,1,1);
            
            sliceDataBuffer0.GetData(slicePolygonData0);
            sliceDataBuffer1.GetData(slicePolygonData1);
            newDotDataBuffer.GetData(newDotData);
            sliceCountBuffer0.GetData(sliceCount0);
            sliceCountBuffer1.GetData(sliceCount1);
            newDotCountBuffer.GetData(newDotCount);
            
            verticesBuffer.Release();
            normalsBuffer.Release();
            uvsBuffer.Release();
            trianglesBuffer.Release();
            dotLengthBuffer.Release();
            
            sliceDataBuffer0.Release();
            sliceDataBuffer1.Release();
            newDotDataBuffer.Release();
            sliceCountBuffer0.Release();
            sliceCountBuffer1.Release();
            newDotCountBuffer.Release();
        }
    }
}

