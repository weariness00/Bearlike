using System;
using System.Collections.Generic;
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
        // Start is called before the first frame update
        void Start()
        {
            MeshDestruction.Destruction(a, PrimitiveType.Cube, Vector3.zero, Vector3.one);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
