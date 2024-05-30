using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class TestMeshSlice : MonoBehaviour
{
    public GameObject sliceTarget;
    public Vector3 point;
    public Vector3 normal;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MeshSlicing.Slice(sliceTarget, normal, point);
        }
    }
}
