using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Test
{
    public class Test2 : MonoBehaviour
    {
        public List<PolygonData[]> polygonData;

        private Mesh mesh;
        
        private void Start()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                polygonData = new List<PolygonData[]>();
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                var uvs = mesh.uv;
                for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
                {
                    var subMesh = mesh.GetSubMesh(subMeshIndex);
                    var indices = mesh.GetIndices(subMeshIndex);
                    PolygonData[] subMeshPolygonData = new PolygonData[indices.Length / 3];
                    
                    for (int polygonIndex = 0; polygonIndex < indices.Length / 3; polygonIndex++)
                    {
                        subMeshPolygonData[polygonIndex].Dot0.Vertex = vertices[indices[polygonIndex * 3]];
                        subMeshPolygonData[polygonIndex].Dot1.Vertex = vertices[indices[polygonIndex * 3 + 1]];
                        subMeshPolygonData[polygonIndex].Dot2.Vertex = vertices[indices[polygonIndex * 3 + 2]];
                    
                        subMeshPolygonData[polygonIndex].Dot0.Normal = normals[indices[polygonIndex * 3]];
                        subMeshPolygonData[polygonIndex].Dot1.Normal = normals[indices[polygonIndex * 3 + 1]];
                        subMeshPolygonData[polygonIndex].Dot2.Normal = normals[indices[polygonIndex * 3 + 2]];
                    
                        subMeshPolygonData[polygonIndex].Dot0.UV = uvs[indices[polygonIndex * 3]];
                        subMeshPolygonData[polygonIndex].Dot1.UV = uvs[indices[polygonIndex * 3 + 1]];
                        subMeshPolygonData[polygonIndex].Dot2.UV = uvs[indices[polygonIndex * 3 + 2]];
                    
                        subMeshPolygonData[polygonIndex].Dot0.Index = indices[polygonIndex * 3];
                        subMeshPolygonData[polygonIndex].Dot1.Index = indices[polygonIndex * 3 + 1];
                        subMeshPolygonData[polygonIndex].Dot2.Index = indices[polygonIndex * 3 + 2];
                    }

                    polygonData.Add(subMeshPolygonData);
                }
            }
        }
        
        #region Data Structure

        /// <summary>
        /// 폴리곤 데이터를 담을 구조체
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PolygonData
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
    }
}