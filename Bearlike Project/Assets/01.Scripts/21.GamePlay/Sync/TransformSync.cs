using UnityEngine;

namespace GamePlay.Sync
{
    public class TransformSync : MonoBehaviour
    {
        public Transform targetTransform; // 동기화 할 대상

        public bool isPosition = true;
        public bool isRotate = true;
        public bool isRotateX = true, isRotateY = true, isRotateZ = true;
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
                var rotate = transform.rotation;
                var targetRotate = targetTransform.rotation;
                if (isRotateX) rotate.x = targetRotate.x;
                if (isRotateY) rotate.y = targetRotate.y;
                if (isRotateZ) rotate.z = targetRotate.z;

                transform.rotation = rotate;
            }

            if (isScale)
            {
                transform.localScale = targetTransform.localScale;
            }
        }
    }
}

