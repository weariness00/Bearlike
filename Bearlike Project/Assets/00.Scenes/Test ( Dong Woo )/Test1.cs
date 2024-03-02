using System.Collections.Generic;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class Test1 : MonoBehaviour
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uv;
        // Start is called before the first frame update
        void Start()
        {
            var meshFilter = GetComponent<MeshFilter>();
            vertices = meshFilter.sharedMesh.vertices;
            normals = meshFilter.sharedMesh.normals;
            uv = meshFilter.sharedMesh.uv;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
