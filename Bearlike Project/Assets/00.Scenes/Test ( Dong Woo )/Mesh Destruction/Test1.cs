using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Manager;
using UnityEngine;
using Util;

namespace Test
{
    public class TestSliceComputeShader : MonoBehaviour
    {
        public int testNumber = 0;
        public Vector3 sliceP;
        public Vector3 sliceN;

        public Material subMat;
        
        public ComputeShader sliceShader;
        
        private struct CSParam
        {
            public const string MeshSliceKernel = "CSMeshSlice";
            public const string MakeCapKernel = "CSMakeCap";
            public const string OptimizeSliceData = "CSOptimizeSliceData";
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
            
            // Cap Data
            public static readonly int FaceDirect = Shader.PropertyToID("faceDirect");
            public static readonly int UVForward = Shader.PropertyToID("uvForward");
            public static readonly int UVLeft = Shader.PropertyToID("uvLeft");
            public static readonly int NewDotCenter = Shader.PropertyToID("newDotCenter");
            
            // Optimize Slice Data
            public static readonly int SlicePolygon = Shader.PropertyToID("slicePolygon");
            public static readonly int SlicePolygonLength = Shader.PropertyToID("slicePolygonLength");
            public static readonly int SliceDotData = Shader.PropertyToID("sliceDotData");
            public static readonly int SliceDotDataLength = Shader.PropertyToID("sliceDotDataLength");
        }
        
        #region Unity Enven Function
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Slice(gameObject, sliceN, sliceP);
            }
            else if (Input.GetKeyDown(KeyCode.S))
                MeshSlicing.Slice(gameObject, sliceN, sliceP, subMat);
        }

        #endregion
        
        #region Data Structure

        /// <summary>
        /// 폴리곤 데이터를 담을 구조체
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct PolygonData
        {
            public DotData Dot0;
            public DotData Dot1;
            public DotData Dot2;
        }

        /// <summary>
        /// Mesh의 한 점의 정보를 담는 자료형
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DotData
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

        public List<GameObject> Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint, bool isDestroyOrigin = true)
        {
            Mesh mesh = targetObject.GetComponent<MeshFilter>().sharedMesh;
            
            // slice Point를 targetObject.Transform.Position 만큼 움직이기
            slicePoint -= targetObject.transform.position;
            
            // slice normal을 targetObject.Transform.Rotate 만큼 회전
            sliceNormal = targetObject.transform.rotation * sliceNormal;
            
            int dotCount = mesh.vertices.Length;
            int triangleCount = mesh.triangles.Length;
            int polygonCount = triangleCount / 3;
            
            ComputeBuffer verticesBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer normalsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 3);
            ComputeBuffer uvsBuffer = new ComputeBuffer(dotCount, sizeof(float) * 2);
            ComputeBuffer trianglesBuffer = new ComputeBuffer(triangleCount, sizeof(int));
            ComputeBuffer dotLengthBuffer = new ComputeBuffer(1, sizeof(uint));
            
            ComputeBuffer sliceDataBuffer0 = new ComputeBuffer(polygonCount * 2, Marshal.SizeOf(typeof(PolygonData)));
            ComputeBuffer sliceDataBuffer1 = new ComputeBuffer(polygonCount * 2, Marshal.SizeOf(typeof(PolygonData)));
            ComputeBuffer newDotDataBuffer = new ComputeBuffer(polygonCount * 2, Marshal.SizeOf(typeof(DotData)));
            ComputeBuffer sliceCountBuffer0 = new ComputeBuffer(1, sizeof(uint));
            ComputeBuffer sliceCountBuffer1 = new ComputeBuffer(1, sizeof(uint));
            ComputeBuffer newDotCountBuffer = new ComputeBuffer(1, sizeof(uint));
                
            PolygonData[] slicePolygonData0 = new PolygonData[polygonCount * 2];
            PolygonData[] slicePolygonData1 = new PolygonData[polygonCount * 2];
            DotData[] newDotData = new DotData[polygonCount * 2];
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
            
            int meshSliceKernelID = sliceShader.FindKernel(CSParam.MeshSliceKernel);
            sliceShader.SetInt(CSParam.PolygonLength, polygonCount);
            sliceShader.SetVector(CSParam.SlicePoint, slicePoint);
            sliceShader.SetVector(CSParam.SliceNormal, sliceNormal);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.Vertices, verticesBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.Normals, normalsBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.UVs, uvsBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.Triangles, trianglesBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.DotLength, dotLengthBuffer);
            
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceData0, sliceDataBuffer0);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceData1, sliceDataBuffer1);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.NewDotData, newDotDataBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceCount0, sliceCountBuffer0);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceCount1, sliceCountBuffer1);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.NewDotCount, newDotCountBuffer);

            sliceShader.Dispatch(meshSliceKernelID, polygonCount / CSParam.ThreadX + 1,1,1);
            
            sliceDataBuffer0.GetData(slicePolygonData0);
            sliceDataBuffer1.GetData(slicePolygonData1);
            newDotDataBuffer.GetData(newDotData);
            sliceCountBuffer0.GetData(sliceCount0);
            sliceCountBuffer1.GetData(sliceCount1);
            newDotCountBuffer.GetData(newDotCount);

            if (newDotCount[0] <= 0)
            {
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
                DebugManager.LogWarning("Slice된 객체가 없습니다.");
                return new List<GameObject>(){targetObject};
            }

            // 새롭게 생긴 정점들 정렬
            // 정렬된 값을 Buffer에 Set
            ComputeBuffer newDotCenterBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(DotData)));

            var sortDots = SortNewDot(newDotData, (int)newDotCount[0]);
            newDotCount[0] = (uint)sortDots.Length - 1; // 배열의 마지막 원소는 첫번째 원소와 값이 동일하기에 길이를 1 빼준다.
            
            newDotDataBuffer.Release();
            newDotDataBuffer = new ComputeBuffer(sortDots.Length, Marshal.SizeOf(typeof(DotData)));
            newDotDataBuffer.SetData(sortDots);
            newDotCountBuffer.SetData(newDotCount);
            
            // slice 폴리곤 갯수 크기 재조정
            Array.Resize(ref slicePolygonData0, (int)sliceCount0[0] + sortDots.Length - 1);
            Array.Resize(ref slicePolygonData1, (int)sliceCount1[0] + sortDots.Length - 1);
            sliceDataBuffer0.Release();
            sliceDataBuffer1.Release();
            sliceDataBuffer0 = new ComputeBuffer(slicePolygonData0.Length, Marshal.SizeOf(typeof(PolygonData)));
            sliceDataBuffer1 = new ComputeBuffer(slicePolygonData1.Length, Marshal.SizeOf(typeof(PolygonData)));
            sliceDataBuffer0.SetData(slicePolygonData0);
            sliceDataBuffer1.SetData(slicePolygonData1);
    
            DotData[] center = new []{new DotData(){Vertex = Vector3.zero, Normal = Vector3.zero, UV = new Vector2(0.5f,0.5f)}};
            for (var i = 0; i < sortDots.Length - 1; i++)
                center[0].Vertex += sortDots[i].Vertex;
            center[0].Vertex /= sortDots.Length - 1;
            center[0].Index = sortDots.Length + dotCount - 1;
            
            float faceDir = Vector3.Dot(sliceNormal, Vector3.Cross(sortDots[0].Vertex - center[0].Vertex, sortDots[1].Vertex - sortDots[0].Vertex));
            
            newDotCenterBuffer.SetData(center);
            
            Vector3 forward = Vector3.zero;
            forward.x = sliceNormal.y;
            forward.y = -sliceNormal.x;
            forward.z = sliceNormal.z;
            Vector3 left = Vector3.Cross(forward, sliceNormal);

            int makeCapKernelID = sliceShader.FindKernel(CSParam.MakeCapKernel);
            sliceShader.SetFloat(CSParam.FaceDirect, faceDir);
            sliceShader.SetVector(CSParam.UVForward, forward);
            sliceShader.SetVector(CSParam.UVLeft, left);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.NewDotCenter, newDotCenterBuffer);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.SliceData0, sliceDataBuffer0);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.SliceData1, sliceDataBuffer1);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.NewDotData, newDotDataBuffer);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.SliceCount0, sliceCountBuffer0);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.SliceCount1, sliceCountBuffer1);
            sliceShader.SetBuffer(makeCapKernelID, CSParam.NewDotCount, newDotCountBuffer);
            
            sliceShader.Dispatch(makeCapKernelID, (sortDots.Length - 1) / CSParam.ThreadX + 1, 1, 1);
            
            sliceDataBuffer0.GetData(slicePolygonData0);
            sliceDataBuffer1.GetData(slicePolygonData1);

            var sliceObject0 = MakeSliceObject(targetObject, MakeMeshFromPolygonData(slicePolygonData0));
            var sliceObject1 = MakeSliceObject(targetObject, MakeMeshFromPolygonData(slicePolygonData1));
                        
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
            
            newDotCenterBuffer.Release();
            
            if(isDestroyOrigin) Destroy(targetObject);

            return new List<GameObject>() { sliceObject0, sliceObject1 };
        }

        private DotData[] SortNewDot(DotData[] newDots, int length)
        {
            var result = new List<DotData>
            {
                newDots[0],
                newDots[1]
            };

            int compareCount = length / 2;
            for (int i = 0; i < compareCount -1; i++)
            {
                for (int j = i + 1; j < compareCount; j++)
                {
                    if (newDots[i * 2 + 1].Vertex == newDots[j * 2].Vertex)
                    {
                        result.Add(newDots[j * 2 + 1]);
                        SwapTwoIndexSet(ref newDots, i * 2 + 2, i * 2 + 3, j * 2, j * 2 + 1);
                    }
                    else if (newDots[i * 2 + 1].Vertex == newDots[j * 2 + 1].Vertex)
                    {
                        result.Add(newDots[j * 2]);
                        SwapTwoIndexSet(ref newDots, i * 2 + 2, i * 2 + 3, j * 2 + 1, j * 2);
                    }
                }
            }
            if (result.First().Vertex != result.Last().Vertex)
                result.Add(result.First());
            return result.ToArray();
        }

        private static void SwapTwoIndexSet<T>(ref T[] target, int idx00, int idx01, int idx10, int idx11)
        {
            T temp0 = target[idx00];
            T temp1 = target[idx01];
            target[idx00] = target[idx10];
            target[idx01] = target[idx11];
            target[idx10] = temp0;
            target[idx11] = temp1;
        }

        private Mesh MakeMeshFromPolygonData(PolygonData[] slicePolygonData)
        {
            HashSet<DotData> dotData = new HashSet<DotData>(new DotDataEqualityComparer());
            foreach (var data in slicePolygonData)
            {
                dotData.Add(data.Dot0);
                dotData.Add(data.Dot1);
                dotData.Add(data.Dot2);
            }

            var arrayDot = dotData.ToArray();
            for (var i = 0; i < arrayDot.Length; i++)
                arrayDot[i].Index = i;
            
            Mesh mesh = new Mesh();
            mesh.SetVertices(dotData.Select(dot => dot.Vertex).ToList());
            mesh.SetNormals(dotData.Select(dot => dot.Normal).ToList());
            mesh.SetUVs(0, dotData.Select(dot => dot.UV).ToList());

            ComputeBuffer slicePolygonBuffer = new ComputeBuffer(slicePolygonData.Length, Marshal.SizeOf(typeof(PolygonData)));
            ComputeBuffer sliceDotDataBuffer = new ComputeBuffer(arrayDot.Length, Marshal.SizeOf(typeof(DotData)));
            
            slicePolygonBuffer.SetData(slicePolygonData);
            sliceDotDataBuffer.SetData(arrayDot);
            
            int kernelID = sliceShader.FindKernel(CSParam.OptimizeSliceData);
            sliceShader.SetInt(CSParam.SlicePolygonLength, slicePolygonData.Length);
            sliceShader.SetInt(CSParam.SliceDotDataLength, dotData.Count);
            sliceShader.SetBuffer(kernelID, CSParam.SlicePolygon, slicePolygonBuffer);
            sliceShader.SetBuffer(kernelID, CSParam.SliceDotData, sliceDotDataBuffer);

            sliceShader.Dispatch(kernelID, slicePolygonData.Length / CSParam.ThreadX + 1, 1, 1);
            
            slicePolygonBuffer.GetData(slicePolygonData);
            
            slicePolygonBuffer.Release();
            sliceDotDataBuffer.Release();
            
            int[] triangles = new int[slicePolygonData.Length * 3];
            for (var i = 0; i < slicePolygonData.Length; i++)
            {
                var polygon = slicePolygonData[i];
                triangles[i * 3 + 0] = polygon.Dot0.Index;
                triangles[i * 3 + 1] = polygon.Dot1.Index;
                triangles[i * 3 + 2] = polygon.Dot2.Index;
            }
            mesh.SetTriangles(triangles, 0);
            
            return mesh;
        }
        
        private GameObject MakeSliceObject(GameObject originObject ,Mesh sliceMesh)
        {
            var originMeshRenderer = originObject.GetComponent<MeshRenderer>();
            
            var components = new[] { typeof(MeshFilter), typeof(MeshRenderer) };

            if (originObject.name.Contains("Slice") == false)
                sliceMesh.name = originObject.name + "Slice_Mesh";
            
            GameObject sliceGameObject = new GameObject(originObject.name, components);
            sliceGameObject.GetComponent<MeshFilter>().sharedMesh = sliceMesh;
            sliceGameObject.GetComponent<MeshRenderer>().sharedMaterials = originMeshRenderer.sharedMaterials;
            sliceGameObject.transform.position = originObject.transform.position;
            sliceGameObject.transform.rotation = originObject.transform.rotation;
            sliceGameObject.transform.localScale = originObject.transform.localScale;

            if (sliceGameObject.name.Contains("Slice") == false)
                sliceGameObject.name += "Slice";
            
            if(originObject.transform.parent) 
                sliceGameObject.transform.SetParent(originObject.transform.parent);

            return sliceGameObject;
        }

        #region Compare Class

        public class DotDataEqualityComparer : IEqualityComparer<DotData>
        {
            private const float epsilon = 1e-5f;

            public bool Equals(DotData x, DotData y)
            {
                return Vector3.Distance(x.Vertex, y.Vertex) < epsilon &&
                       Vector3.Distance(x.Normal, y.Normal) < epsilon;
            }

            public int GetHashCode(DotData obj)
            {
                // 더 나은 해시 분포를 위해 각 컴포넌트를 적절히 스케일링
                int hash = 17;
                hash = hash * 23 + (obj.Vertex.x).GetHashCode();
                hash = hash * 23 + (obj.Vertex.y).GetHashCode();
                hash = hash * 23 + (obj.Vertex.z).GetHashCode();
                hash = hash * 23 + (obj.Normal.x).GetHashCode();
                hash = hash * 23 + (obj.Normal.y).GetHashCode();
                hash = hash * 23 + (obj.Normal.z).GetHashCode();
                return hash;
            }
        }
        
        #endregion
    }
}

