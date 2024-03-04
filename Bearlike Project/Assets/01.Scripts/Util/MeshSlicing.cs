using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using DebugManager = Script.Manager.DebugManager;
using Object = UnityEngine.Object;

namespace Util
{
    public class MeshSlicing
    {
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

            public void AddRange([NotNull] SliceInfo other)
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
        struct PolygonData
        {
            public PolygonData(int arrayIndex = 0)
            {
                Dots = new MeshDotData[3];
            }

            public MeshDotData[] Dots;
        }

        /// <summary>
        /// Mesh의 한 점의 정보를 담는 자료형
        /// </summary>
        public struct MeshDotData
        {
            public MeshDotData(Vector3 v, Vector3 n, Vector3 u)
            {
                Vertex = v;
                Normal = n;
                UV = u;
            }
            public Vector3 Vertex;
            public Vector3 Normal;
            public Vector2 UV;
        }

        // 선분
        struct Parametric
        {
            public Parametric(Vector3 p1, Vector3 p2)
            {
                A = p1;
                B = p2;
            }
            
            public Vector3 A;
            public Vector3 B;

            public float Magnitude() => Vector3.Magnitude(B - A);

            public bool TryGetPoint(float t, out Vector3 point)
            {
                Vector3 direction = A - B; // 방향 벡터 계산
                point = A + t * direction; // 파라메트릭 방정식
                
                if (A.x < point.x && point.x < B.x)
                    return true;
                else
                    return false;
            }
        }
        
        #endregion

        #region Default Function

        static GameObject CreateSliceGameObject(GameObject targetObject, SliceInfo sliceInfo)
        {
            var targetMesh = targetObject.GetComponent<MeshFilter>();
            var targetMeshRenderer = targetObject.GetComponent<MeshRenderer>();
            
            // pivot 중심으로 다시 잡아주기
            Vector3 center = new Vector3(
                sliceInfo.DotList.Average(point => point.Vertex.x),
                sliceInfo.DotList.Average(point => point.Vertex.y),
                sliceInfo.DotList.Average(point => point.Vertex.z));
            for (var i = 0; i < sliceInfo.DotList.Count; i++)
            {
                var dot = sliceInfo.DotList[i];
                dot.Vertex -= center;
                sliceInfo.DotList[i] = dot;
            }
            
            Mesh mesh = new Mesh
            {
                subMeshCount = targetMeshRenderer.sharedMaterials.Length + 1,
                name = targetMesh.name + "_Slicing",
                vertices = sliceInfo.DotList.Select(dot => dot.Vertex).ToArray(),
                normals = sliceInfo.DotList.Select(dot => dot.Normal).ToArray(),
                uv = sliceInfo.DotList.Select(dot => dot.UV).ToArray()
            };
            mesh.SetTriangles(sliceInfo.Triangles, 0);
            DebugManager.Log("나중에 subMesh에 포함하는 형식으로 하여 Material 개별 적용 가능하게 바꾸기");
            // mesh.SetTriangles(capSliceInfos[i].triangles, targetMeshRenderer.sharedMaterials.Length);
                
            GameObject sliceGameObject = new GameObject(targetObject.name + "_Slicing", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody));
            var collider = sliceGameObject.GetComponent<MeshCollider>();
            collider.convex = true;
            collider.sharedMesh = mesh;
            sliceGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            sliceGameObject.GetComponent<MeshRenderer>().sharedMaterials = targetMeshRenderer.sharedMaterials;
            sliceGameObject.tag = "Destruction";
            sliceGameObject.transform.position = targetObject.transform.position + center;
            sliceGameObject.transform.rotation = targetObject.transform.rotation;
            sliceGameObject.transform.localScale = targetObject.transform.localScale;
            
            if(targetObject.transform.parent) sliceGameObject.transform.SetParent(targetObject.transform.parent);

           return sliceGameObject;
        }

        #endregion

        #region Slice Function
        
        /// <summary>
        /// 메쉬를 2개로 잘라준다.
        /// </summary>
        /// <param name="targetObject">자를 메쉬</param>
        /// <param name="sliceNormal">자를 단면의 평면 노멀값</param>
        /// <param name="slicePoint">자를 단면의 평면 위의 한 점</param>
        public static GameObject[] Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint)
        {
            var targetMesh = targetObject.GetComponent<MeshFilter>().sharedMesh;
            SliceInfo[] sliceInfos = new []{new SliceInfo(), new SliceInfo()};
            SliceInfo createdSliceInfo = new SliceInfo();

            // 폴리곤 갯수만큼 반복
            int polygonCount = targetMesh.triangles.Length / 3;
            for (int i = 0; i < polygonCount; i++)
            {
                // 폴리곤의 정보를 가져온다.
                PolygonData polygonData = new PolygonData(0);
                int[] vertexIndices = new int[3];
                float[] dots = new float[3]; // 단면이 바라보는 방향인지 판단하기 위한 내적
                for (int j = 0; j < 3; j++)
                {
                    var vertexIndex = vertexIndices[j] = targetMesh.triangles[i * 3 + j];
                    polygonData.Dots[j].Vertex = targetMesh.vertices[vertexIndex];
                    polygonData.Dots[j].Normal = targetMesh.normals[vertexIndex];
                    polygonData.Dots[j].UV = targetMesh.uv[vertexIndex];

                    dots[j] = Vector3.Dot(sliceNormal, polygonData.Dots[j].Vertex - slicePoint);
                }

                // 단면의 바라보는 반대 방향에 있을떄
                if (dots[0] < 0 && dots[1] < 0 && dots[2] < 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sliceInfos[0].DotList.Add(polygonData.Dots[j]);
                        sliceInfos[0].Triangles.Add(sliceInfos[0].Triangles.Count);
                    }
                }
                // 단면 바라보는 방향에 있을떄
                else if (dots[0] >= 0 && dots[1] >= 0 && dots[2] >= 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sliceInfos[1].DotList.Add(polygonData.Dots[j]);
                        sliceInfos[1].Triangles.Add(sliceInfos[1].Triangles.Count);
                    }
                }
                // 각 정점이 한 방향에 있지 않을때
                else
                {
                    // 1 : 2 로 정점이 나뉜다.
                    // 혼자 있는 정점 Index : 0
                    // 같이 있는 정점 Index : 1, 2
                    float epsilon = 1e-5f;
                    int[] otherVertexIndices = new[]
                    {
                        Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[1])) < epsilon ? vertexIndices[2] : (Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[2])) < epsilon ? vertexIndices[1] : vertexIndices[0]),
                        Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[1])) < epsilon ? vertexIndices[0] : (Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[2])) < epsilon ? vertexIndices[2] : vertexIndices[1]),
                        Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[1])) < epsilon ? vertexIndices[1] : (Math.Abs(Mathf.Sign(dots[0]) - Mathf.Sign(dots[2])) < epsilon ? vertexIndices[0] : vertexIndices[2]),
                    };
                    
                    PolygonData otherPolygonData = new PolygonData(0);
                    float[] otherToPlaneDistances = new float[3];
                    float[] ratios = new float[3]; // 0번째 원소는 의미 없는 값이다.
                    for (int j = 0; j < 3; j++)
                    {
                        otherPolygonData.Dots[j].Vertex = targetMesh.vertices[otherVertexIndices[j]];
                        otherPolygonData.Dots[j].Normal = targetMesh.normals[otherVertexIndices[j]];
                        otherPolygonData.Dots[j].UV = targetMesh.uv[otherVertexIndices[j]];
                        otherToPlaneDistances[j] = Mathf.Abs(Vector3.Dot(sliceNormal, otherPolygonData.Dots[j].Vertex - slicePoint));
                        ratios[j] = otherToPlaneDistances[0] / (otherToPlaneDistances[0] + otherToPlaneDistances[j]);
                    }

                    // 슬라이스 메쉬 정보를 추가
                    for (int j = 1; j < 3; j++)
                    {
                        createdSliceInfo.DotList.Add(new MeshDotData(
                            Vector3.Lerp(otherPolygonData.Dots[0].Vertex, otherPolygonData.Dots[j].Vertex, ratios[j]),
                            Vector3.Lerp(otherPolygonData.Dots[0].Normal, otherPolygonData.Dots[0].Normal, ratios[j]),
                            Vector2.Lerp(otherPolygonData.Dots[0].UV, otherPolygonData.Dots[0].UV, ratios[j])));
                    }
                    
                    // 혼자 있는 정점 위치에 따라 메쉬 정보에 추가
                    float sideDot = Vector3.Dot(sliceNormal, otherPolygonData.Dots[0].Vertex - slicePoint);
                    {
                        // 원소 순서를 뜻하는게 아님 그냥 대략적인 이름
                        int firstSliceInfoIndex = sideDot < 0 ? 0 : 1;
                        int secondSliceInfoIndex = sideDot < 0 ? 1 : 0;
                        
                        // first
                        sliceInfos[firstSliceInfoIndex].DotList.Add(otherPolygonData.Dots[0]);
                        sliceInfos[firstSliceInfoIndex].DotList.Add(createdSliceInfo.DotList[^2]);
                        sliceInfos[firstSliceInfoIndex].DotList.Add(createdSliceInfo.DotList[^1]);
                        sliceInfos[firstSliceInfoIndex].Triangles.Add(sliceInfos[firstSliceInfoIndex].Triangles.Count);
                        sliceInfos[firstSliceInfoIndex].Triangles.Add(sliceInfos[firstSliceInfoIndex].Triangles.Count);
                        sliceInfos[firstSliceInfoIndex].Triangles.Add(sliceInfos[firstSliceInfoIndex].Triangles.Count);
                        
                        //second
                        sliceInfos[secondSliceInfoIndex].DotList.Add(otherPolygonData.Dots[1]);
                        sliceInfos[secondSliceInfoIndex].DotList.Add(otherPolygonData.Dots[2]);
                        sliceInfos[secondSliceInfoIndex].DotList.Add(createdSliceInfo.DotList[^2]);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                        
                        sliceInfos[secondSliceInfoIndex].DotList.Add(otherPolygonData.Dots[2]);
                        sliceInfos[secondSliceInfoIndex].DotList.Add(createdSliceInfo.DotList[^1]);
                        sliceInfos[secondSliceInfoIndex].DotList.Add(createdSliceInfo.DotList[^2]);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                        sliceInfos[secondSliceInfoIndex].Triangles.Add(sliceInfos[secondSliceInfoIndex].Triangles.Count);
                    }
                }
            }
            if (createdSliceInfo.DotList.Count == 0) return Array.Empty<GameObject>();

            createdSliceInfo.DotList = SortVertices(createdSliceInfo.DotList);
            var capSliceInfos = MakeCap(sliceNormal,createdSliceInfo.DotList);
            
            // 최종적으로 사용할 메쉬
            var finalSliceInfos = new[] { new SliceInfo (), new SliceInfo() };
            for (int i = 0; i < 2; i++)
            {
                finalSliceInfos[i].AddRange(sliceInfos[i]);
                finalSliceInfos[i].AddRange(capSliceInfos[i]);
            }
            
            var sliceObjects = new GameObject[2];
            for (int i = 0; i < 2; i++)
            {
                sliceObjects[i] = CreateSliceGameObject(targetObject, finalSliceInfos[i]);
            }
            Object.Destroy(targetObject);

            return sliceObjects;
        }

        public static SliceInfo[] MakeCap(Vector3 sliceNormal, List<MeshDotData> relatedDotList)
        {
            SliceInfo[] sliceInfos = new []{new SliceInfo(), new SliceInfo()};
            int prevSliceInfoCount = sliceInfos.Length;
            sliceInfos[0].DotList.AddRange(relatedDotList);
            sliceInfos[1].DotList.AddRange(relatedDotList);
            if (relatedDotList.Count < 2)
            {
                return sliceInfos;
            }
            
            Vector3 center =  relatedDotList.Aggregate(Vector3.zero, (acc, data) => acc + data.Vertex) / relatedDotList.Count;
            sliceInfos[0].DotList.Add(new MeshDotData(center, Vector3.zero, new Vector2(0.5f, 0.5f)));
            sliceInfos[1].DotList.Add(new MeshDotData(center, Vector3.zero, new Vector2(0.5f, 0.5f)));
            //Calculate cap data
            //Normal
            for (int i = 0; i < relatedDotList.Count + 1; i++)  
            {
                var n1 = sliceInfos[0].DotList[i];
                var n2 = sliceInfos[1].DotList[i];
                n1.Normal = sliceNormal;
                n2.Normal = -sliceNormal;
                sliceInfos[0].DotList[i] = n1;
                sliceInfos[1].DotList[i] = n2;
            }
            //Uv
            //Basis on sliced plane
            Vector3 forward = Vector3.zero;
            forward.x = sliceNormal.y;
            forward.y = -sliceNormal.x;
            forward.z = sliceNormal.z;
            Vector3 left = Vector3.Cross(forward, sliceNormal);
            for (int i = 0; i < relatedDotList.Count; i++)
            {
                Vector3 dir = relatedDotList[i].Vertex - center;
                Vector2 relatedUV = Vector2.zero;
                relatedUV.x = 0.5f + Vector3.Dot(dir, left);
                relatedUV.y = 0.5f + Vector3.Dot(dir, forward);
                var n1 = sliceInfos[0].DotList[i];
                var n2 = sliceInfos[1].DotList[i];
                n1.UV = relatedUV;
                n2.UV = relatedUV;
                sliceInfos[0].DotList[i] = n1;
                sliceInfos[1].DotList[i] = n2;
            }
            //Triangle
            int centerIdx = sliceInfos[0].DotList.Count - 1;
            //Check first triangle face where
            float faceDir = Vector3.Dot(sliceNormal, Vector3.Cross(relatedDotList[0].Vertex - center, relatedDotList[1].Vertex - relatedDotList[0].Vertex));
            //Store tris
            for (int i = 0; i < sliceInfos[0].DotList.Count - 1; i++)
            {
                int idx0 = i;
                int idx1 = (i + 1) % (sliceInfos[0].DotList.Count - 1);
                if (faceDir < 0)
                {
                    sliceInfos[0].Triangles.Add(centerIdx);
                    sliceInfos[0].Triangles.Add(idx1);
                    sliceInfos[0].Triangles.Add(idx0);

                    sliceInfos[1].Triangles.Add(centerIdx);
                    sliceInfos[1].Triangles.Add(idx0);
                    sliceInfos[1].Triangles.Add(idx1);
                }
                else
                {
                    sliceInfos[0].Triangles.Add(centerIdx);
                    sliceInfos[0].Triangles.Add(idx0);
                    sliceInfos[0].Triangles.Add(idx1);

                    sliceInfos[1].Triangles.Add(centerIdx);
                    sliceInfos[1].Triangles.Add(idx1);
                    sliceInfos[1].Triangles.Add(idx0);
                }
            }
            return sliceInfos;
        }

        public static List<MeshDotData> SortVertices(List<MeshDotData> dotList)
        {
            var result = new List<MeshDotData>();
            result.Add(dotList[0]);
            result.Add(dotList[1]);

            int compareCount = dotList.Count / 2;
            for (int i = 0; i < compareCount -1; i++)
            {
                for (int j = i + 1; j < compareCount; j++)
                {
                    if (dotList[i * 2 + 1].Vertex == dotList[j * 2].Vertex)
                    {
                        result.Add(dotList[j * 2 + 1]);
                        dotList.TrySwap(i * 2 + 2, j * 2 , out var e1);
                        dotList.TrySwap(i * 2 + 3, j * 2 + 1 , out var e2);
                    }
                    else if (dotList[i * 2 + 1].Vertex == dotList[j * 2 + 1].Vertex)
                    {
                        result.Add(dotList[j * 2]);
                        dotList.TrySwap(i * 2 + 2, j * 2 + 1, out var e1);
                        dotList.TrySwap(i * 2 + 3, j * 2 , out var e2);
                    }
                }
            }
            if (result.First().Vertex == result.Last().Vertex)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }
        
        #endregion
    }
}