using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    public enum CameraMode
    {
        FirstPerson,    // 1인칭
        ThirdPerson,    // 3인칭
        Free,           // 자유
    }
    
    public class PlayerCameraController : NetworkBehaviour
    {
        public GameObject ownerObject;
        public CameraMode mode = CameraMode.FirstPerson;
        [Header("카메라")] 
        public Camera targetCamera;
        public Camera weaponCamera;
        public Vector3 offset;
        public float firstOffset = 1f;
        public float thirdOffset = -5f;

        public override void Spawned()
        {
            if (HasInputAuthority == false)
            {
                targetCamera.GetComponent<AudioListener>().enabled = false;
                targetCamera.enabled = false;
                weaponCamera.enabled = false;

                return;
            }

            targetCamera.tag = "MainCamera";

            SetOwnerCamera();
            WeaponClipping();
        }

        public void ChangeCameraMode(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.FirstPerson:
                    SetFirstPersonCamera();
                    break;
                case CameraMode.ThirdPerson:
                    SetThirdPersonCamera();
                    break;
            }
        }

        private void SetOwnerCamera()
        {
            if(ownerObject == null) return;
            
            Transform targetCameraTransform = targetCamera.transform;
            Transform ownerTransform = ownerObject.transform;
            targetCameraTransform.SetParent(ownerTransform);
            targetCameraTransform.position = ownerTransform.position + offset;
            targetCameraTransform.rotation = ownerTransform.rotation;
        }

        private void SetFirstPersonCamera()
        {
            Transform targetCameraTransform = targetCamera.transform;

            targetCameraTransform.position = Vector3.zero;
            targetCameraTransform.position = thirdOffset * targetCameraTransform.forward;
        }

        private void SetThirdPersonCamera()
        {
            Transform targetCameraTransform = targetCamera.transform;
            
            targetCameraTransform.position = Vector3.zero;
            targetCameraTransform.position = thirdOffset * targetCameraTransform.forward;
        }

        public void WeaponClipping()
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(weaponCamera);
        }
    }
}