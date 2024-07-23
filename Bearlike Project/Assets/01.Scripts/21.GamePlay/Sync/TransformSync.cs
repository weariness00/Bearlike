using System.ComponentModel;
using UnityEngine;

namespace GamePlay.Sync
{
    public class TransformSync : MonoBehaviour
    {
        public Transform targetTransform; // 동기화 할 대상

        public Vector3 positionOffset;
        public float positionMultiple = 1f;
        
        public bool isPosition = true;
        public bool isPositionLocal = false;
        public bool isPositionX = true, isPositionY = true, isPositionZ = true;
        public bool isRotate = true;
        public bool isRotateLocal = false;
        public bool isRotateX = true, isRotateY = true, isRotateZ = true;
        public bool isScale = false;

        private Vector3 sourceForward = Vector3.forward; // 소스 오브젝트의 Forward 축
        private Vector3 sourceUp = Vector3.up; // 소스 오브젝트의 Up 축
        private Vector3 targetForward = Vector3.up; // 타겟 오브젝트의 Forward 축 (예: Y)
        private Vector3 targetUp = Vector3.forward; // 타겟 오브젝트의 Up 축 (예: Z)
        private void LateUpdate()
        {
            TransformSyncUpdate();
        }

        public void TransformSyncUpdate()
        {
            if (!targetTransform)
            {
                return;
            }
            
            if (isPosition)
            {
                Vector3 realPosition = Vector3.zero;
                Vector3 targetPosition = Vector3.zero;
                if (isPositionLocal) targetPosition = targetTransform.localPosition;
                else targetPosition = targetTransform.position;

                if (isPositionX) realPosition.x = targetPosition.x;
                if (isPositionY) realPosition.y = targetPosition.y;
                if (isPositionZ) realPosition.z = targetPosition.z;
                
                transform.position = (realPosition * positionMultiple) + positionOffset;
            }

            if (isRotate)
            {
                var rotate = transform.rotation;
                var targetRotate = Quaternion.identity;

                if (isRotateLocal) targetRotate = targetTransform.localRotation;
                else targetRotate = targetTransform.rotation;
                
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

