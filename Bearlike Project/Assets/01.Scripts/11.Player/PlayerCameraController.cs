using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DebugManager = Manager.DebugManager;

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
        public Camera uiCamera;
        public Vector3 firstOffset;
        public Vector3 thirdOffset;

        private Volume _globalVolume;

        private void Awake()
        {
            _globalVolume = FindObjectsOfType<Volume>().First(v => v.isGlobal);
        }

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

        #region Volume Function

        public void SetActiveLensDistortion(bool value) 
        {
            if (_globalVolume.profile.TryGet(out LensDistortion lensDistortion))
                lensDistortion.active = value;
            else
                DebugManager.LogWarning("Lens Distortion이 없습니다.");
        }

        public void SetLensDistortion(
            float intensity = 0f, 
            float multiplierX = 1f,
            float multiplierY = 1f,
            Vector2? center = null,
            float scale = 1f, 
            float durationTime = 0f)
        {
            if (_globalVolume.profile.TryGet(out LensDistortion lensDistortion))
            {
                center ??= new Vector2(0.5f, 0.5f);

                if (durationTime == 0f)
                {
                    lensDistortion.intensity.value = intensity;
                    lensDistortion.xMultiplier.value = multiplierX;
                    lensDistortion.yMultiplier.value = multiplierY;
                    lensDistortion.center.value = center.Value;
                    lensDistortion.scale.value = scale;
                }
                else
                {
                    if(_lensDistortionDurationCoroutine != null) StopCoroutine(_lensDistortionDurationCoroutine); 
                    _lensDistortionDurationCoroutine = StartCoroutine(LensDistortionDurationCoroutine(lensDistortion, intensity, multiplierX, multiplierY, center.Value, scale, durationTime));
                }
            }
            else
                DebugManager.LogWarning("Lens Distortion이 없습니다.");
        }

        private Coroutine _lensDistortionDurationCoroutine;
        private IEnumerator LensDistortionDurationCoroutine(
            LensDistortion lensDistortion,
            float intensity, 
            float multiplierX,
            float multiplierY,
            Vector2 center,
            float scale, 
            float durationTime)
        {
            var timer = 0f;
            var normalizeTime = 0f;

            float originIntensity = lensDistortion.intensity.value;
            float originXMultiplier = lensDistortion.xMultiplier.value;
            float originYMultiplier = lensDistortion.yMultiplier.value;
            Vector2 originCenter = lensDistortion.center.value;
            float originScale = lensDistortion.scale.value;
            while (timer < durationTime)
            {
                timer += Time.deltaTime;
                normalizeTime = timer / durationTime;

                lensDistortion.intensity.value = Mathf.Lerp(originIntensity, intensity, normalizeTime);
                lensDistortion.xMultiplier.value = Mathf.Lerp(originIntensity, originXMultiplier, normalizeTime);
                lensDistortion.yMultiplier.value = Mathf.Lerp(originIntensity, originYMultiplier, normalizeTime);
                lensDistortion.center.value = Vector2.Lerp(originCenter, center, normalizeTime);
                lensDistortion.scale.value = Mathf.Lerp(originScale, scale, normalizeTime);
                
                yield return null;
            }
        }
        
        #endregion


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