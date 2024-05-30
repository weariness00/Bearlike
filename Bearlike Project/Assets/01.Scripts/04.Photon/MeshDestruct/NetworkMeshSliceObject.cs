using System;
using System.Collections;
using UnityEngine;

namespace Photon.MeshDestruct
{
    public class NetworkMeshSliceObject : MonoBehaviour
    {
        public bool isSlice = true;
        public bool isHasMesh = false;

        private MeshFilter _meshFilter;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            StartCoroutine(CheckHasMeshCoroutine());
        }

        IEnumerator CheckHasMeshCoroutine()
        {
            while (true)
            {
                if (_meshFilter.sharedMesh != null)
                {
                    break;
                }

                yield return null;
            }

            isSlice = false;
            isHasMesh = true;
        }
    }
}

