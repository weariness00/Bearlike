using System;
using Fusion;
using Script.Photon;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkPrefabRef p;

    private void Start()
    {
        FindObjectOfType<NetworkRunner>().Spawn(p, Vector3.zero, Quaternion.identity);
    }

    public override void FixedUpdateNetwork()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Runner.Spawn(p, Vector3.zero, Quaternion.identity);
        }
    }
}

