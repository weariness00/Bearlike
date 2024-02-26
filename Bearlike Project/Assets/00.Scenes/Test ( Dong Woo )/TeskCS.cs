using System;
using UnityEngine;
using Util;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class TeskCS : MonoBehaviour
    {
        private void Start()
        {
            MeshSlicing.SliceBoxRange(
                gameObject, Vector3.zero, 0.5f, 0.5f, Vector3.forward
            );
            // MeshSlicing.Slice(gameObject, new Vector3(0.5f,0.5f,0f), gameObject.transform.position);
        }
    }
}
