using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class MeshSlicing
    {
        /// <summary>
        /// 쪼개진 mesh 정보들을 담을 클래스
        /// </summary>
        class SliceInfo
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector2> uv = new List<Vector2>();
            public List<int> triangles = new List<int>();
        }

        /// <summary>
        /// 폴리곤 데이터를 담을 구조체
        /// </summary>
        struct PolygonData
        {
            public PolygonData(int arrayIndex = 0)
            {
                vertex = arrayIndex == 0 ? new Vector3[3] : new Vector3[arrayIndex];
                normal = arrayIndex == 0 ? new Vector3[3] : new Vector3[arrayIndex];
                uv = arrayIndex == 0 ? new Vector2[3] : new Vector2[arrayIndex];
            }
            
            public Vector3[] vertex;
            public Vector3[] normal;
            public Vector2[] uv;
        }
        
        /// <summary>
        /// 메쉬를 2개로 잘라준다.
        /// </summary>
        /// <param name="targetObject">자를 메쉬</param>
        /// <param name="sliceNormal">자를 단면의 평면 노멀값</param>
        /// <param name="slicePoint">자를 단면의 평면 위의 한 점</param>
        public static void Slice(GameObject targetObject, Vector3 sliceNormal, Vector3 slicePoint)
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
                    polygonData.vertex[j] = targetMesh.vertices[vertexIndex];
                    polygonData.normal[j] = targetMesh.normals[vertexIndex];
                    polygonData.uv[j] = targetMesh.uv[vertexIndex];

                    dots[j] = Vector3.Dot(sliceNormal, polygonData.vertex[j] - slicePoint);
                }

                // 단면의 바라보는 반대 방향에 있을떄
                if (dots[0] < 0 && dots[1] < 0 && dots[2] < 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sliceInfos[0].vertices.Add(polygonData.vertex[j]);
                        sliceInfos[0].normals.Add(polygonData.normal[j]);
                        sliceInfos[0].uv.Add(polygonData.uv[j]);
                        sliceInfos[0].triangles.Add(sliceInfos[0].triangles.Count);
                    }
                }
                // 단면 바라보는 방향에 있을떄
                else if (dots[0] >= 0 && dots[1] >= 0 && dots[2] >= 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sliceInfos[1].vertices.Add(polygonData.vertex[j]);
                        sliceInfos[1].normals.Add(polygonData.normal[j]);
                        sliceInfos[1].uv.Add(polygonData.uv[j]);
                        sliceInfos[1].triangles.Add(sliceInfos[1].triangles.Count);
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
                        otherPolygonData.vertex[j] = targetMesh.vertices[otherVertexIndices[j]];
                        otherPolygonData.normal[j] = targetMesh.normals[otherVertexIndices[j]];
                        otherPolygonData.uv[j] = targetMesh.uv[otherVertexIndices[j]];
                        otherToPlaneDistances[j] = Mathf.Abs(Vector3.Dot(sliceNormal, otherPolygonData.vertex[j] - slicePoint));
                        ratios[j] = otherToPlaneDistances[0] / (otherToPlaneDistances[0] + otherToPlaneDistances[j]);
                    }

                    // 슬라이스 메쉬 정보를 추가
                    for (int j = 1; j < 3; j++)
                    {
                        createdSliceInfo.vertices.Add(Vector3.Lerp(otherPolygonData.vertex[0], otherPolygonData.vertex[j], ratios[j]));
                        createdSliceInfo.normals.Add(Vector3.Lerp(otherPolygonData.normal[0], otherPolygonData.normal[j], ratios[j]));
                        createdSliceInfo.uv.Add(Vector3.Lerp(otherPolygonData.uv[0], otherPolygonData.uv[j], ratios[j]));
                    }
                    
                    // 혼자 있는 정점 위치에 따라 메쉬 정보에 추가
                    float sideDot = Vector3.Dot(sliceNormal, otherPolygonData.vertex[0] - slicePoint);
                    {
                        // 원소 순서를 뜻하는게 아님 그냥 대략적인 이름
                        int firstSliceInfoIndex = sideDot < 0 ? 0 : 1;
                        int secondSliceInfoIndex = sideDot < 0 ? 1 : 0;
                        
                        // first
                        sliceInfos[firstSliceInfoIndex].vertices.Add(otherPolygonData.vertex[0]);
                        sliceInfos[firstSliceInfoIndex].vertices.Add(createdSliceInfo.vertices[^2]);
                        sliceInfos[firstSliceInfoIndex].vertices.Add(createdSliceInfo.vertices[^1]);
                        sliceInfos[firstSliceInfoIndex].normals.Add(otherPolygonData.normal[0]);
                        sliceInfos[firstSliceInfoIndex].normals.Add(createdSliceInfo.normals[^2]);
                        sliceInfos[firstSliceInfoIndex].normals.Add(createdSliceInfo.normals[^1]);
                        sliceInfos[firstSliceInfoIndex].uv.Add(otherPolygonData.uv[0]);
                        sliceInfos[firstSliceInfoIndex].uv.Add(createdSliceInfo.uv[^2]);
                        sliceInfos[firstSliceInfoIndex].uv.Add(createdSliceInfo.uv[^1]);
                        sliceInfos[firstSliceInfoIndex].triangles.Add(sliceInfos[firstSliceInfoIndex].triangles.Count);
                        sliceInfos[firstSliceInfoIndex].triangles.Add(sliceInfos[firstSliceInfoIndex].triangles.Count);
                        sliceInfos[firstSliceInfoIndex].triangles.Add(sliceInfos[firstSliceInfoIndex].triangles.Count);
                        
                        //second
                        sliceInfos[secondSliceInfoIndex].vertices.Add(otherPolygonData.vertex[1]);
                        sliceInfos[secondSliceInfoIndex].vertices.Add(otherPolygonData.vertex[2]);
                        sliceInfos[secondSliceInfoIndex].vertices.Add(createdSliceInfo.vertices[^2]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(otherPolygonData.normal[1]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(otherPolygonData.normal[2]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(createdSliceInfo.normals[^2]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(otherPolygonData.uv[1]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(otherPolygonData.uv[2]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(createdSliceInfo.uv[^2]);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                        
                        sliceInfos[secondSliceInfoIndex].vertices.Add(otherPolygonData.vertex[2]);
                        sliceInfos[secondSliceInfoIndex].vertices.Add(createdSliceInfo.vertices[^1]);
                        sliceInfos[secondSliceInfoIndex].vertices.Add(createdSliceInfo.vertices[^2]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(otherPolygonData.normal[2]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(createdSliceInfo.normals[^1]);
                        sliceInfos[secondSliceInfoIndex].normals.Add(createdSliceInfo.normals[^2]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(otherPolygonData.uv[2]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(createdSliceInfo.uv[^1]);
                        sliceInfos[secondSliceInfoIndex].uv.Add(createdSliceInfo.uv[^2]);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                        sliceInfos[secondSliceInfoIndex].triangles.Add(sliceInfos[secondSliceInfoIndex].triangles.Count);
                    }
                }
            }

            // 새로운 오브젝트 생성
            for (int i = 0; i < 2; i++)
            {
                Mesh mesh = new Mesh
                {
                    name = targetMesh.name + "_Slicing",
                    vertices = sliceInfos[i].vertices.ToArray(),
                    normals = sliceInfos[i].normals.ToArray(),
                    uv = sliceInfos[i].uv.ToArray()
                };
                mesh.SetTriangles(sliceInfos[i].triangles, 0);

                GameObject sliceGameObject = new GameObject(targetObject.name + "_Slicing", typeof(MeshFilter), typeof(MeshRenderer));
                sliceGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                sliceGameObject.GetComponent<MeshRenderer>().sharedMaterials = targetObject.GetComponent<MeshRenderer>().sharedMaterials;
                sliceGameObject.transform.position = targetObject.transform.position;
                sliceGameObject.transform.rotation = targetObject.transform.rotation;
                sliceGameObject.transform.localScale = targetObject.transform.localScale;
            }
            targetObject.SetActive(false);
        }
    }
}