using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using DebugManager = Manager.DebugManager;
using Object = UnityEngine.Object;

namespace Util
{
     public class MeshSlicing
    {
        private static ComputeShader sliceShader;
        
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

            public static readonly int SubMeshIndices = Shader.PropertyToID("subMeshIndices");
            
            // Cap Data
            public static readonly int UVForward = Shader.PropertyToID("uvForward");
            public static readonly int UVLeft = Shader.PropertyToID("uvLeft");
            public static readonly int NewDotCenter = Shader.PropertyToID("newDotCenter");
            
            // Optimize Slice Data
            public static readonly int SlicePolygon = Shader.PropertyToID("slicePolygon");
            public static readonly int SlicePolygonLength = Shader.PropertyToID("slicePolygonLength");
            public static readonly int SliceDotData = Shader.PropertyToID("sliceDotData");
            public static readonly int SliceDotDataLength = Shader.PropertyToID("sliceDotDataLength");
        }
        
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

        private static void LoadShader()
        {
            if (!sliceShader)
            {
                sliceShader = Resources.Load<ComputeShader>("91.ComputeShader/MeshDataConvertComputeShader");
            }
        }
        
        public static List<GameObject> Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint, bool isDestroyOrigin = true)
        {
            LoadShader();
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
            dotLengthBuffer.SetData(dotLength);
            
            sliceDataBuffer0.SetData(slicePolygonData0);
            sliceDataBuffer1.SetData(slicePolygonData1);
            newDotDataBuffer.SetData(newDotData);
            sliceCountBuffer0.SetData(sliceCount0);
            sliceCountBuffer1.SetData(sliceCount1);
            newDotCountBuffer.SetData(newDotCount);
            
            sliceShader.SetVector(CSParam.SlicePoint, slicePoint);
            sliceShader.SetVector(CSParam.SliceNormal, sliceNormal);
            
            int meshSliceKernelID = sliceShader.FindKernel(CSParam.MeshSliceKernel);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.Vertices, verticesBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.Normals, normalsBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.UVs, uvsBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.DotLength, dotLengthBuffer);
            
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceData0, sliceDataBuffer0);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceData1, sliceDataBuffer1);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.NewDotData, newDotDataBuffer);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceCount0, sliceCountBuffer0);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.SliceCount1, sliceCountBuffer1);
            sliceShader.SetBuffer(meshSliceKernelID, CSParam.NewDotCount, newDotCountBuffer);

            ComputeBuffer indicesBuffer = null;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var indices = mesh.GetIndices(i);
                var subMeshPolygonCount = indices.Length / 3;

                indicesBuffer = new ComputeBuffer(indices.Length, sizeof(int));
                indicesBuffer.SetData(indices);
                sliceShader.SetInt(CSParam.PolygonLength, subMeshPolygonCount);
                sliceShader.SetBuffer(meshSliceKernelID, CSParam.SubMeshIndices, indicesBuffer);

                sliceShader.Dispatch(meshSliceKernelID, subMeshPolygonCount / CSParam.ThreadX + 1,1,1);
                
                // 컴퓨트 쉐이더 끝나는 시점 알기 위한 함수
                newDotCountBuffer.GetData(newDotCount);
                
                indicesBuffer.Release();
            }
            
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
                dotLengthBuffer.Release();
            
                sliceDataBuffer0.Release();
                sliceDataBuffer1.Release();
                newDotDataBuffer.Release();
                sliceCountBuffer0.Release();
                sliceCountBuffer1.Release();
                newDotCountBuffer.Release();
                
                indicesBuffer.Release();
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
            float maxDis = 0f;
            for (var i = 1; i < sortDots.Length - 1; i++)
            {
                var dis = Vector3.Distance(sortDots[i].Vertex, sortDots[0].Vertex);
                if (maxDis < dis)
                {
                    maxDis = dis;
                    center[0].Vertex = sortDots[i].Vertex;
                }
            }
            center[0].Vertex = Vector3.Lerp(center[0].Vertex, sortDots[0].Vertex, 0.5f);
            center[0].Index = sortDots.Length + dotCount - 1;
            
            newDotCenterBuffer.SetData(center);
            
            Vector3 forward = Vector3.zero;
            forward.x = sliceNormal.y;
            forward.y = -sliceNormal.x;
            forward.z = sliceNormal.z;
            Vector3 left = Vector3.Cross(forward, sliceNormal);

            int makeCapKernelID = sliceShader.FindKernel(CSParam.MakeCapKernel);
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

            var sliceObject0 = MakeSliceObject(targetObject, MakeMeshFromPolygonData(slicePolygonData0, sortDots.Length - 1));
            var sliceObject1 = MakeSliceObject(targetObject, MakeMeshFromPolygonData(slicePolygonData1, sortDots.Length - 1));
                        
            verticesBuffer.Release();
            normalsBuffer.Release();
            uvsBuffer.Release();
            dotLengthBuffer.Release();
            
            sliceDataBuffer0.Release();
            sliceDataBuffer1.Release();
            newDotDataBuffer.Release();
            sliceCountBuffer0.Release();
            sliceCountBuffer1.Release();
            newDotCountBuffer.Release();
            
            indicesBuffer.Release();
            
            newDotCenterBuffer.Release();
            
            if(isDestroyOrigin) Object.Destroy(targetObject);

            return new List<GameObject>() { sliceObject0, sliceObject1 };
        }

        private static DotData[] SortNewDot(DotData[] newDots, int length)
        {
            var result = new List<DotData>
            {
                newDots[0],
                newDots[1]
            };
            
            int compareCount = length / 2;
            for (int i = 0; i < compareCount; i++)
            {
                for (int j = i + 1; j < compareCount; j++)
                {
                    var dot0 = newDots[j * 2];
                    var dot1 = newDots[j * 2 + 1];
                    if (CompareVector3(newDots[i * 2 + 1].Vertex, dot0.Vertex) && 
                        CompareVector3(newDots[i * 2 + 1].Vertex, dot1.Vertex) == false)
                    {
                        result.Add(dot1);
                        SwapTwoIndexSet(ref newDots, i * 2 + 2, i * 2 + 3, j * 2, j * 2 + 1);
                    }
                    else if (CompareVector3(newDots[i * 2 + 1].Vertex, dot1.Vertex) &&
                            CompareVector3(newDots[i * 2 + 1].Vertex, dot0.Vertex) == false)
                    {
                        result.Add(dot0);
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
        
        private static Mesh MakeMeshFromPolygonData(PolygonData[] slicePolygonData, int newPolygonCount)
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
            
            int[] triangles = new int[(slicePolygonData.Length - newPolygonCount) * 3];
            int[] sliceMeshTriangles = new int[newPolygonCount * 3];
            for (var i = 0; i < triangles.Length / 3; i++)
            {
                var polygon = slicePolygonData[i];
                triangles[i * 3 + 0] = polygon.Dot0.Index;
                triangles[i * 3 + 1] = polygon.Dot1.Index;
                triangles[i * 3 + 2] = polygon.Dot2.Index;
            }
            for (int i = 0; i < sliceMeshTriangles.Length / 3; i++)
            {
                var polygon = slicePolygonData[triangles.Length / 3 + i];
                sliceMeshTriangles[i * 3 + 0] = polygon.Dot0.Index;
                sliceMeshTriangles[i * 3 + 1] = polygon.Dot1.Index;
                sliceMeshTriangles[i * 3 + 2] = polygon.Dot2.Index;
            }

            mesh.subMeshCount = 2;
            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(sliceMeshTriangles, 1);
            
            return mesh;
        }
        
        private static GameObject MakeSliceObject(GameObject originObject ,Mesh sliceMesh)
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

        #region Compare

        public static bool CompareVector3(Vector3 a, Vector3 b)
        {
            const float epsilon = 0.01f;
            return Vector3.Distance(a, b) < epsilon;
        }
        
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