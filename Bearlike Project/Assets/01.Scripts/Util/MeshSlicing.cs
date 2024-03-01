using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using DebugManager = Script.Manager.DebugManager;

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
                
            GameObject sliceGameObject = new GameObject(targetObject.name + "_Slicing", typeof(MeshFilter), typeof(MeshRenderer));
            sliceGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            sliceGameObject.GetComponent<MeshRenderer>().sharedMaterials = targetMeshRenderer.sharedMaterials;
            sliceGameObject.transform.position = targetObject.transform.position;
            sliceGameObject.transform.rotation = targetObject.transform.rotation;
            sliceGameObject.transform.localScale = targetObject.transform.localScale;

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

            createdSliceInfo.DotList = SortVertices(createdSliceInfo.DotList);
            var capSliceInfos = MakeCap(sliceNormal,createdSliceInfo.DotList);
            
            // 최종적으로 사용할 메쉬
            var finalSliceInfos = new[] { new SliceInfo (), new SliceInfo() };
            for (int i = 0; i < 2; i++)
            {
                finalSliceInfos[i].AddRange(sliceInfos[i]);
                finalSliceInfos[i].AddRange(capSliceInfos[i]);
            }
            
            var targetMeshRenderer = targetObject.GetComponent<MeshRenderer>();
            var sliceObjects = new GameObject[2];
            // 새로운 오브젝트 생성
            for (int i = 0; i < 2; i++)
            {
                Mesh mesh = new Mesh
                {
                    subMeshCount = targetMeshRenderer.sharedMaterials.Length + 1,
                    name = targetMesh.name + "_Slicing",
                    vertices = finalSliceInfos[i].DotList.Select(dot => dot.Vertex).ToArray(),
                    normals = finalSliceInfos[i].DotList.Select(dot => dot.Normal).ToArray(),
                    uv = finalSliceInfos[i].DotList.Select(dot => dot.UV).ToArray()
                };
                mesh.SetTriangles(finalSliceInfos[i].Triangles, 0);
                DebugManager.Log("나중에 subMesh에 포함하는 형식으로 하여 Material 개별 적용 가능하게 바꾸기");
                // mesh.SetTriangles(capSliceInfos[i].triangles, targetMeshRenderer.sharedMaterials.Length);
                
                GameObject sliceGameObject = new GameObject(targetObject.name + "_Slicing", typeof(MeshFilter), typeof(MeshRenderer));
                sliceGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                sliceGameObject.GetComponent<MeshRenderer>().sharedMaterials = targetMeshRenderer.sharedMaterials;
                sliceGameObject.transform.position = targetObject.transform.position;
                sliceGameObject.transform.rotation = targetObject.transform.rotation;
                sliceGameObject.transform.localScale = targetObject.transform.localScale;

                sliceObjects[i] = sliceGameObject;
            }
            targetObject.SetActive(false);

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

        #region Slice Box Rnage
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="rectPoint"></param>
        /// <param name="width">사각형 영역의 길이</param>
        /// <param name="height">사각형 영역의 높이</param>
        /// <param name="rectNormal"> 잘리게 될 폴리곤의 normal 벡터 </param>
        /// <returns></returns>
        public static GameObject[] SliceBoxRange(
            GameObject targetObject, Vector3 rectPoint, float width, float height, Vector3 rectNormal
        )
        {
            var targetMesh = targetObject.GetComponent<MeshFilter>().sharedMesh;
            SliceInfo[] sliceInfos = new[] { new SliceInfo(), new SliceInfo() };
            SliceInfo capSliceInfo = new SliceInfo();
            SliceInfo boxCapSliceInfo = new SliceInfo();

            // rectNormal 벡터가 주어졌을 때, right와 up 벡터 계산
            Vector3 up = Vector3.up; // 기본적으로 세계 좌표계의 위쪽 방향을 사용
            Vector3 right = Vector3.Cross(up, rectNormal).normalized; // right 벡터 계산
            up = Vector3.Cross(rectNormal, right).normalized; // 수정된 up 벡터를 계산하여 forward와 정확히 직교하도록 함

            //Rectangle의 각 점들을 구한다.
            var rectPoints = new[]
            {
                rectPoint + height * -up / 2 + width * -right / 2, // 왼쪽 아래 점
                rectPoint + height * up / 2 + width * -right / 2, // 왼쪽 위 점
                rectPoint + height * up / 2 + width * right / 2, // 오른쪽 위 점
                rectPoint + height * -up / 2 + width * right / 2, // 오른쪽 아래 점
            };
            var rectNormals = new[] { -right, up, right, -up }; // 왼쪽 위 오른쪽 아래 순

            // 3차원 평면 방정식
            Parametric[] lineSegments = new Parametric[4];
            lineSegments[0].A = rectPoints[0];
            lineSegments[0].B = rectPoints[1];
            lineSegments[1].A = rectPoints[1];
            lineSegments[1].B = rectPoints[2];
            lineSegments[2].A = rectPoints[2];
            lineSegments[2].B = rectPoints[3];
            lineSegments[3].A = rectPoints[3];
            lineSegments[3].B = rectPoints[0];
            
            // 폴리곤 갯수만큼 반복
            int polygonCount = targetMesh.triangles.Length / 3;
            for (int polygonIndex = 0; polygonIndex < polygonCount; polygonIndex++)
            {
                // 폴리곤의 정보를 가져온다.
                PolygonData polygonData = new PolygonData(0);
                int[] vertexIndices = new int[3];
                float[] dots = new float[3]; // 단면이 바라보는 방향인지 판단하기 위한 내적
                for (int i = 0; i < 3; i++)
                {
                    var vertexIndex = vertexIndices[i] = targetMesh.triangles[polygonIndex * 3 + i];
                    polygonData.Dots[i].Vertex = targetMesh.vertices[vertexIndex];
                    polygonData.Dots[i].Normal = targetMesh.normals[vertexIndex];
                    polygonData.Dots[i].UV = targetMesh.uv[vertexIndex];
                }

                // 각 선분마다 검사를 진행 해야됨
                for (int lineSegmentIndex = 0; lineSegmentIndex < 4; lineSegmentIndex++)
                {
                    var lineSegment = lineSegments[lineSegmentIndex];
                    var sliceNormal = rectNormals[lineSegmentIndex];
                    var slicePoint = lineSegment.A;
                    for (int i = 0; i < 3; i++)
                    {
                        dots[i] = Vector3.Dot(sliceNormal, polygonData.Dots[i].Vertex - slicePoint);
                    }

                    // 단면이 바라보는 반대 방향에 있을떄
                    if (dots[0] <= 0 && dots[1] <= 0 && dots[2] <= 0) continue;
                    // 단면이 바라보는 방향에 폴리곤의 정점이 3개 다 있을떄
                    if (dots[0] > 0 && dots[1] > 0 && dots[2] > 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            sliceInfos[0].DotList.Add(polygonData.Dots[i]);
                            sliceInfos[0].Triangles.Add(sliceInfos[0].DotList.Count - 1);
                        }
                        break;
                    }
                    // 정점이 좌우로 나뉘어 졌을떄
                    else
                    {
                        float epsilon = 1e-5f;
                        
                        // 0 번째 원소 : Rectangle의 한 선분의 Normal 벡터의 반대 방향에 있는 정점
                        // 1, 2 번째 원소 : Rectangle의 한 선분의 Normal 벡터의 방향에 있는 정점
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

                        // Rectangle의 한 선분과 교차하는 정점의 정보를 추가
                        MeshDotData[] crossDots = new MeshDotData[2];
                        for (int i = 1; i < 3; i++)
                        {
                            MeshDotData dotData = new MeshDotData(
                                Vector3.Lerp(otherPolygonData.Dots[0].Vertex, otherPolygonData.Dots[i].Vertex, ratios[i]),
                                Vector3.Lerp(otherPolygonData.Dots[0].Normal, otherPolygonData.Dots[i].Normal, ratios[i]),
                                Vector2.Lerp(otherPolygonData.Dots[0].UV, otherPolygonData.Dots[i].UV, ratios[i]));
                            
                            // createdSliceInfo.DotList.Add(dot);
                            crossDots[i - 1] = dotData;
                        }
                        // 폴리곤과 평면의 교차점이 선분을 평면으로 만든 영역에 있는지 체크
                        if(IsPolygonCrossDotInLineSegmentPlane(crossDots[0].Vertex,crossDots[1].Vertex, lineSegment.A, lineSegment.B)) continue;
                        
                        // 정점이 선분의 범위에서 벗어나는지 체크해야됨
                        var dot = crossDots[0];
                        var otherDot = crossDots[1];
                        if (IsInPointFromPlaneLimit(dot.Vertex, lineSegment.A, lineSegment.B, out var newVertex1))
                        {
                            var ratio = (dot.Vertex - newVertex1).magnitude / (dot.Vertex - otherDot.Vertex).magnitude;
                            var uv = Vector2.Lerp(dot.UV, otherDot.UV, ratio);
                            var newDot = new MeshDotData(newVertex1, dot.Normal, uv);
                            capSliceInfo.DotList.Add(newDot);
                            boxCapSliceInfo.DotList.Add(newDot);
                            boxCapSliceInfo.DotList.Add(new (newDot.Vertex, sliceNormal, newDot.UV));
                        }
                        else
                        {
                            capSliceInfo.DotList.Add(dot);
                            boxCapSliceInfo.DotList.Add(dot);
                            boxCapSliceInfo.DotList.Add(new (dot.Vertex, sliceNormal, dot.UV));
                        }
                            
                        if (IsInPointFromPlaneLimit(otherDot.Vertex, lineSegment.A, lineSegment.B, out var newVertex2))
                        {
                            var ratio = (dot.Vertex - newVertex2).magnitude / (dot.Vertex - otherDot.Vertex).magnitude;
                            var uv = Vector2.Lerp(dot.UV, otherDot.UV, ratio);
                            var newDot = new MeshDotData(newVertex2, dot.Normal, uv);
                            capSliceInfo.DotList.Add(newDot);   
                            boxCapSliceInfo.DotList.Add(newDot); 
                            boxCapSliceInfo.DotList.Add(new (newDot.Vertex, sliceNormal, newDot.UV));
                        }
                        else
                        {
                            capSliceInfo.DotList.Add(otherDot);
                            boxCapSliceInfo.DotList.Add(otherDot);
                            boxCapSliceInfo.DotList.Add(new (otherDot.Vertex, sliceNormal, otherDot.UV));
                        }
                        
                        // 바라보는 점 추가
                        // 단면이 바라보는 방향이 아닌 정점은 무시
                        int inDotCount = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            if (dots[i] > 0)
                            {
                                ++inDotCount;
                            }
                        }
                        
                        SliceInfo subSliceInfo = new SliceInfo();
                        if (inDotCount == 1)
                        {
                            subSliceInfo.DotList.Add(otherPolygonData.Dots[0]);
                            subSliceInfo.DotList.Add(capSliceInfo.DotList[^2]);
                            subSliceInfo.DotList.Add(capSliceInfo.DotList[^1]);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                        }
                        // 바라보는 방향에 점이 2개이면 모든 점의 중심점을 만들고 각 점들을 정렬한 뒤 중심점과 연결된 폴리곤 만들기
                        else if (inDotCount == 2)
                        {
                            subSliceInfo.DotList.Add(otherPolygonData.Dots[1]);
                            subSliceInfo.DotList.Add(otherPolygonData.Dots[2]);
                            subSliceInfo.DotList.Add(capSliceInfo.DotList[^2]);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);

                            subSliceInfo.DotList.Add(otherPolygonData.Dots[2]);
                            subSliceInfo.DotList.Add(capSliceInfo.DotList[^1]);
                            subSliceInfo.DotList.Add(capSliceInfo.DotList[^2]);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                            subSliceInfo.Triangles.Add(subSliceInfo.Triangles.Count);
                        }
                        
                        sliceInfos[0].AddRange(subSliceInfo);
                    }
                }
            }
            
            // createdSliceInfo.vertices = SortVertices(createdSliceInfo.vertices);
            // var capSliceInfos = MakeCap(sliceNormal,createdSliceInfo.vertices);
            
            
            // 최종적으로 사용할 메쉬
            var finalSliceInfos = new[] { new SliceInfo(), new SliceInfo() };
            for (int i = 0; i < 2; i++)
            {
                finalSliceInfos[i].AddRange(sliceInfos[i]);
                // finalSliceInfos[i].AddRange(capSliceInfos[i]);
            }

            // 새로운 오브젝트 생성
            GameObject[] sliceObjects = new[]
            {
                CreateSliceGameObject(targetObject, finalSliceInfos[0]),
                CreateSliceGameObject(targetObject, MakeBoxCap(boxCapSliceInfo)), // 새롭게 생긴 정점들로 만들어주는 잘리고 남은 box 메쉬
            };
            targetObject.SetActive(false);

            return sliceObjects;
        }

        static SliceInfo MakeBoxCap(SliceInfo boxSliceInfo)
        {
            GameObject gameObject;
            SliceInfo sortSliceInfo = new SliceInfo();
            Dictionary<Vector3, List<MeshDotData>> sameNormalDotDictionary = new Dictionary<Vector3, List<MeshDotData>>();
            float tolerance = 1E-4f;

            // 같은 평면에 있는 정점끼리 묶기
            for (var i = 0; i < boxSliceInfo.DotList.Count; i++)
            {
                var dot = boxSliceInfo.DotList[i];
                // 소수점 4자리 부터는 반올림
                dot.Vertex.x = (float)Math.Round(dot.Vertex.x, 4);
                dot.Vertex.y = (float)Math.Round(dot.Vertex.y, 4);
                dot.Vertex.z = (float)Math.Round(dot.Vertex.z, 4);
                
                dot.Normal.x = (float)Math.Round(dot.Normal.x, 4);
                dot.Normal.y = (float)Math.Round(dot.Normal.y, 4);
                dot.Normal.z = (float)Math.Round(dot.Normal.z, 4);

                if (sameNormalDotDictionary.TryGetValue(dot.Normal, out var meshDotDataList))
                {
                    meshDotDataList.Add(dot);
                }
                else
                {
                    meshDotDataList = new List<MeshDotData>();
                    meshDotDataList.Add(dot);
                    sameNormalDotDictionary.Add(dot.Normal, meshDotDataList);
                }
            }
            
            // 같은 평면 상에 놓인 점들의 중심점을 찾아 정렬한뒤 폴리곤 만들기
            foreach (var (normal, dotList) in sameNormalDotDictionary)
            {
                // vector값이 같은 것들은 제거
                var dots = dotList.GroupBy(dot => dot.Vertex).Select(group => group.First()).ToList();
                
                var subSliceInfo = new SliceInfo();
                // 중심점 찾기
                Vector3 center = new Vector3(
                    dots.Average(point => point.Vertex.x),
                    dots.Average(point => point.Vertex.y),
                    dots.Average(point => point.Vertex.z));
                Vector2 uv = new Vector2(
                    dots.Average(point => point.UV.x),
                    dots.Average(point => point.UV.y));
                var centerDot = new MeshDotData(center, normal, uv);

                // 평면에 대한 로컬 2D 좌표계 생성
                Vector3 referencePoint = dots[0].Vertex; // 기준점으로 첫 번째 정점을 사용
                Vector3 arbitraryDirection = (referencePoint - center).normalized;
                Vector3 u = arbitraryDirection; // 평면에 평행한 첫 번째 방향 벡터
                Vector3 v = Vector3.Cross(normal, u); // 평면에 평행하고 u에 수직인 두 번째 방향 벡터

                // 2D 투영과 원본 정점의 매핑 생성
                var mapping = dotList.Select(dot => new {
                    dot = dot,
                    Projection = new Vector2(Vector3.Dot(dot.Vertex - center, u), Vector3.Dot(dot.Vertex - center, v))
                }).ToList();

                // Projection을 기준으로 매핑 정렬
                var sortedMapping = mapping.OrderBy(item => Mathf.Atan2(item.Projection.y, item.Projection.x)).ToList();

                // 정렬된 매핑에서 원본 3D 정점 추출
                dots = sortedMapping.Select(item => item.dot).ToList();
  
                // center 정점 추가
                float faceDir = Vector3.Dot(normal, Vector3.Cross(dots[0].Vertex - center, dots[1].Vertex - dots[0].Vertex));
                for (int i = 0; i < dots.Count; i++)
                {
                    int idx0 = i;
                    int idx1 = (i + 1) % (dots.Count);
                    if (faceDir < 0)
                    {
                        subSliceInfo.Triangles.Add(dots.Count);
                        subSliceInfo.Triangles.Add(idx1);
                        subSliceInfo.Triangles.Add(idx0);
                    }
                    else
                    {
                        subSliceInfo.Triangles.Add(dots.Count);
                        subSliceInfo.Triangles.Add(idx0);
                        subSliceInfo.Triangles.Add(idx1);
                    }
                }
                dots.Add(centerDot);
                subSliceInfo.DotList = dots;
                
                sortSliceInfo.AddRange(subSliceInfo);
            }

            return sortSliceInfo;
        }

        /// <summary>
        /// 점 2개의 선분 사이의 평면으로 제한된 점을 찾기 위한 함수
        /// </summary>
        /// <param name="p"> 평면위의 점 </param>
        /// <param name="a"> 제한 할 점 A</param>
        /// <param name="b">제한 할 점 B</param>
        /// <param name="newPoint"> 점 A,B에 의해 제한되어 재정의 된 p</param>
        /// <returns>true : 제한됨, false : 제한 안됨</returns>
        static bool IsInPointFromPlaneLimit(Vector3 p, Vector3 a, Vector3 b, out Vector3 newPoint)
        {
            newPoint = Vector3.zero;
            if (b - a == Vector3.zero)
                return false;
            bool isX = b.x - a.x != 0f;
            bool isY = b.y - a.y != 0f;
            bool isZ = b.z - a.z != 0f;

            newPoint = p;
            float min, max;
            if (isX)
            {
                min = a.x > b.x ? b.x : a.x;
                max = a.x < b.x ? b.x : a.x;
                newPoint.x = Mathf.Clamp(p.x, min, max);
            }
            if (isY)
            {
                min = a.y > b.y ? b.y : a.y;
                max = a.y < b.y ? b.y : a.y;
                newPoint.y = Mathf.Clamp(p.y, min, max);
            }
            if (isZ)
            {
                min = a.z > b.z ? b.z : a.z;
                max = a.z < b.z ? b.z : a.z;
                newPoint.z = Mathf.Clamp(p.z, min, max);
            }
            return newPoint.x != p.x || newPoint.y != p.y || newPoint.z != p.z;
        }

        /// <summary>
        /// 슬라이스에 의해 잘린 폴리곤에 생긴 점2개가 선분을 평면으로 만든 영역안에 존재하는지 체크
        /// </summary>
        /// <param name="p1">평면과 교차하는 점</param>
        /// <param name="p2">평면과 교차하는 점</param>
        /// <param name="a">선분의 점 a</param>
        /// <param name="b">선분의 점 b</param>
        /// <returns>True : 선분의 평면 영역 박, False : 선분의 평면 영역 안</returns>
            static bool IsPolygonCrossDotInLineSegmentPlane(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
            {
                if (b - a == Vector3.zero)
                    return false;
                bool isX = b.x - a.x != 0f;
                bool isY = b.y - a.y != 0f;
                bool isZ = b.z - a.z != 0f;

                float min, max;
                if (isX)
                {
                    min = a.x > b.x ? b.x : a.x;
                    max = a.x < b.x ? b.x : a.x;
                    return (p1.x <= min && p2.x <= min) || (p1.x >= max && p2.x >= max);
                }
                if (isY)
                {
                    min = a.y > b.y ? b.y : a.y;
                    max = a.y < b.y ? b.y : a.y;
                    return (p1.y <= min && p2.y <= min) || (p1.y >= max && p2.y >= max);
                }
                if (isZ)
                {
                    min = a.z > b.z ? b.z : a.z;
                    max = a.z < b.z ? b.z : a.z;
                    return (p1.z <= min && p2.z <= min) || (p1.z >= max && p2.z >= max);
                }
                return false;
            }
        #endregion
    }
}