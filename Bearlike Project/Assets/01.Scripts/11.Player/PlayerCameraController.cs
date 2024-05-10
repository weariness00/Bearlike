using System.Collections;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

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
        public Vector3 firstOffset;
        public Vector3 thirdOffset;

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
            SetFirstPersonCamera();
        }

        private void SetFirstPersonCamera()
        {
            Transform targetCameraTransform = targetCamera.transform;

            targetCameraTransform.localPosition = Vector3.zero;
            targetCameraTransform.localPosition = firstOffset;
            
            weaponCamera.enabled = true;
        }

        private void SetThirdPersonCamera()
        {
            Transform targetCameraTransform = targetCamera.transform;
            
            targetCameraTransform.localPosition = Vector3.zero;
            targetCameraTransform.localPosition = thirdOffset;

            weaponCamera.enabled = false;
        }

        public void WeaponClipping()
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(weaponCamera);
        }

        public void ShakeCamera(float duration, Vector3 strength, int vibrato, float randomness = 0.0f)
        {
            targetCamera.transform.DOShakePosition(duration, strength, vibrato, randomness);
            targetCamera.transform.DOShakeRotation(duration, strength, vibrato, randomness);
        }

        public void ReboundCamera()
        {
            StartCoroutine(ReboundCameraBackCoroutine());
        }

        private IEnumerator ReboundCameraBackCoroutine()
        {
            var movement = 0.0f;
            while (true)
            {
                movement += 0.5f;
                targetCamera.fieldOfView += 0.5f;
                yield return new WaitForSeconds(0.01f);
                if (movement >= 4.0f)
                {
                    StartCoroutine(ReboundCameraFrontCoroutine());
                    yield break;
                }
            }
        }
        
        private IEnumerator ReboundCameraFrontCoroutine()
        {
            var movement = 0.0f;
            while (true)
            {
                movement += 0.5f;
                targetCamera.fieldOfView -= 0.5f;
                yield return new WaitForSeconds(0.01f);
                if (movement >= 4.0f)
                {
                    yield break;
                }
            }
        }
    }
}