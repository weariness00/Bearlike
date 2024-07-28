using System;
using System.Collections;
using Photon;
using UnityEngine;

public class NaMeshRestart : MonoBehaviour
{
    public GameObject[] navMeshLinkers;

    private bool isActive = false;

    public void Start()
    {
        Destroy(gameObject, 5.0f);
    }

    public void Update()
    {
        if (!isActive)
        {
            foreach (var linker in navMeshLinkers)
            {
                linker.SetActive(false);
                linker.SetActive(true);
            }

            // isActive = true;
        }
    }
}
