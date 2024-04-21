using System;
using UnityEngine;

namespace Photon.MeshDestruct
{
    public class NetworkMeshDestructObject : MonoBehaviour
    {
        private static int ObjectCounting = 0;
        public int id;

        private void Awake()
        {
            id = ObjectCounting++;
        }

        public void OnApplicationQuit()
        {
            ObjectCounting = 0;
        }
    }
}

