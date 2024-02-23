using System;
using UnityEngine;
using Util;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class TeskCS : MonoBehaviour
    {
        public void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Slice"))
            {
                MeshSlicing.Slice(other.gameObject, Vector3.right, gameObject.transform.position);
            }
        }
    }
}
