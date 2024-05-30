using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDistance : MonoBehaviour
{
    public GameObject Pig;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{(Pig.transform.position - transform.position).magnitude}");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"{(Pig.transform.position - transform.position).magnitude}");
    }
}
