using System;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace GamePlay.Sync
{
    public class TransformSync : MonoBehaviour
    {
        public Transform targetTransform; // 동기화 할 대상

        public bool isPosition = true;
        public bool isRotate = true;
        public bool isScale = false;

        private void Update()
        {
            TransformSyncUpdate();
        }

        public void TransformSyncUpdate()
        {
            if (targetTransform == null)
            {
                return;
            }
            
            if (isPosition)
            {
                transform.position = targetTransform.position;
            }

            if (isRotate)
            {
                transform.rotation = targetTransform.rotation;
            }

            if (isScale)
            {
                transform.localScale = targetTransform.localScale;
            }
        }
    }
}

