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
            if(Input.GetKeyDown(KeyCode.A))
                MeshSlicing.Slice(gameObject, new Vector3(0.5f, 0.5f,0), slicePoint);
        }
    }
}
