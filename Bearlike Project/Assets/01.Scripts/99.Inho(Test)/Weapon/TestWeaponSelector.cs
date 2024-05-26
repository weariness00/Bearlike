using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWeaponSelector : MonoBehaviour
{
    public GameObject[] guns;
    
    void Start()
    {
        foreach (var gun in guns)
        {
            gun.SetActive(false);
        }
        guns[0].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var gun in guns)
            {
                gun.SetActive(false);
            }
            guns[0].SetActive(true);
            guns[0].transform.Find("SmokeVFX").gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var gun in guns)
            {
                gun.SetActive(false);
            }
            guns[1].SetActive(true);            
            guns[1].transform.Find("SmokeVFX").gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            foreach (var gun in guns)
            {
                gun.SetActive(false);
            }
            guns[2].SetActive(true);
            guns[2].transform.Find("SmokeVFX").gameObject.SetActive(false);
        }
    }
}
