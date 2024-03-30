using System;
using UnityEngine;

namespace Photon
{
    [RequireComponent(typeof(MeshCollider))]
    public class NetworkMeshDestructObject : NetworkBehaviourEx
    {
        private void Start()
        {
            GetComponent<MeshCollider>().convex = true;
        }
    }
}