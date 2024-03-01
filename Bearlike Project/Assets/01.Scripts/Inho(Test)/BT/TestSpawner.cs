using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject prefab;

    public int Count;
    
    void Start()
    {
        for(int i = 0; i < Count; i++)
            Instantiate(prefab, new Vector3(i * 10, 0, 0), new Quaternion(0, 0, 0, 0));
    }
}
