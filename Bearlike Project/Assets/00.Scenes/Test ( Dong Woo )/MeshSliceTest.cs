using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class MeshSliceTest : MonoBehaviour
{
    public TestType t;
    public Vector3 slicePoint;

    public enum TestType
    {
        Slice,
    }

    // Update is called once per frame
    void Update()
    {
        if (t == TestType.Slice)
        {
        }
    }
}
