using System;
using System.Collections.Generic;
using System.Linq;
using Parabox.CSG;
using Script.Manager;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class Test1 : MonoBehaviour
    {
        public GameObject a;
        public GameObject b;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                (a, b) = (b, a);
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                var m = CSG.Subtract(a, b);
                var copyMesh = m.mesh;
                var centerVertex = new Vector3(
                    copyMesh.vertices.Average(vertex => vertex.x),
                    copyMesh.vertices.Average(vertex => vertex.y),
                    copyMesh.vertices.Average(vertex => vertex.z)
                );
                var vertices = copyMesh.vertices;
                for (var i = 0; i < vertices.Length; i++)
                    vertices[i] -= centerVertex;
                copyMesh.SetVertices(vertices);
                copyMesh.RecalculateNormals();
                copyMesh.RecalculateBounds();
                var obj = new GameObject(a.name, typeof(MeshFilter), typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().sharedMesh = copyMesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials = m.materials.ToArray();
                obj.transform.position += centerVertex;
            }
            else if(Input.GetKeyDown(KeyCode.S))
            {
                var m = CSG.Intersect(a, b);
                var copyMesh = m.mesh;
                var centerVertex = new Vector3(
                    copyMesh.vertices.Average(vertex => vertex.x),
                    copyMesh.vertices.Average(vertex => vertex.y),
                    copyMesh.vertices.Average(vertex => vertex.z)
                );
                var vertices = copyMesh.vertices;
                for (var i = 0; i < vertices.Length; i++)
                    vertices[i] -= centerVertex;
                copyMesh.SetVertices(vertices);
                copyMesh.RecalculateNormals();
                copyMesh.RecalculateBounds();
                var obj = new GameObject(a.name, typeof(MeshFilter), typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().sharedMesh = copyMesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials = m.materials.ToArray();
                obj.transform.position += centerVertex;
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                var m = CSG.Union(a, b);
                var obj = new GameObject(a.name, typeof(MeshFilter), typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().sharedMesh = m.mesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials = m.materials.ToArray();
            }
        }
    }
}
