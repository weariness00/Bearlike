using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
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
        public Camera skillCamera;
        public Camera fullScreenCamera;
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
                skillCamera.enabled = false;

                return;
            }

            targetCamera.tag = "MainCamera";

            SetOwnerCamera();
            TargetCameraAddOverlay(weaponCamera);
            TargetCameraAddOverlay(skillCamera);            
            TargetCameraAddOverlay(fullScreenCamera);

            StartCoroutine(WeaponCameraShake());
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
            skillCamera.enabled = true;
        }

        private void SetThirdPersonCamera()
        {
            Transform targetCameraTransform = targetCamera.transform;
            
            targetCameraTransform.localPosition = Vector3.zero;
            targetCameraTransform.localPosition = thirdOffset;

            weaponCamera.enabled = false;
            skillCamera.enabled = false;
        }

        public void TargetCameraAddOverlay(Camera overlayCamera)
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(overlayCamera);
        }
        
        public void TargetCameraAddOverlay(int index, Camera overlayCamera)
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Insert(index, overlayCamera);
        }
        
        private Tween _shakePositionTween;
        private Tween _shakeRotationTween;

        public void StopShake()
        {
            _shakePositionTween?.Kill();
            _shakeRotationTween?.Kill();
        }
        public void ShakeCamera(float duration, Vector3 strength, int vibrato, float randomness = 0.0f)
        {
            _shakePositionTween?.Kill();
            _shakeRotationTween?.Kill();

            _shakePositionTween = targetCamera.transform.DOShakePosition(duration, strength, vibrato, randomness);
            _shakeRotationTween = targetCamera.transform.DOShakeRotation(duration, strength, vibrato, randomness);
        }

        public void ReboundCamera()
        {
            StartCoroutine(ReboundCameraBackCoroutine());
        }

        public void ScreenHitImpact(float duration, float boundsThreshold)
        {
            SetChromaticAberration();
            SetVignette();
            SetVignetteColor(Color.red);

            SetChromaticAberration(1f, duration, boundsThreshold);
            SetVignette(0.2f, 1, false, null, duration, boundsThreshold);
        }

        #region Volume Function

        #region Lens Distortion

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
                lensDistortion.xMultiplier.value = Mathf.Lerp(originXMultiplier, multiplierX, normalizeTime);
                lensDistortion.yMultiplier.value = Mathf.Lerp(originYMultiplier, multiplierY, normalizeTime);
                lensDistortion.center.value = Vector2.Lerp(originCenter, center, normalizeTime);
                lensDistortion.scale.value = Mathf.Lerp(originScale, scale, normalizeTime);
                
                yield return null;
            }
        }
        #endregion

        #region Chromatic Aberration Function

        public void SetActiveChromaticAberration(bool value)
        {
            if (_globalVolume.profile.TryGet(out ChromaticAberration profile))
                profile.active = value;
            else
                DebugManager.LogWarning("Chromatic Aberration이 없습니다.");
        }

        public void SetChromaticAberration(float intensity = 0f, float duration = 0f, float boundsThreshold = 0f)
        {
            if (_globalVolume.profile.TryGet(out ChromaticAberration profile))
            {
                if(_chromaticAberrationCoroutine != null) StopCoroutine(_chromaticAberrationCoroutine);
                if(duration == 0f)
                    profile.intensity.value = intensity;
                else
                    _chromaticAberrationCoroutine = StartCoroutine(SetChromaticAberrationCoroutine(profile, intensity, duration, boundsThreshold));
            }
            else
                DebugManager.LogWarning("Chromatic Aberration이 없습니다.");
        }

        private Coroutine _chromaticAberrationCoroutine;
        private IEnumerator SetChromaticAberrationCoroutine(
            ChromaticAberration profile,
            float intensity,
            float duration,
            float boundsThreshold)
        {
            float timer = 0f;
            float originIntensity = profile.intensity.value;
            float normalizeTime = 0f;
            boundsThreshold = boundsThreshold == 0f ? 0.5f : boundsThreshold;
            float normalizeBoundThreshold = Mathf.PI / (duration / boundsThreshold);
            while (timer < duration)
            {
                timer += Time.deltaTime;
                normalizeTime = Mathf.Abs(Mathf.Sin(normalizeBoundThreshold * timer));
                profile.intensity.value = Mathf.Lerp(originIntensity, intensity, normalizeTime);

                yield return null;
            }
        }

        #endregion

        #region Vignette Function
        
        public void SetActiveVignette(bool value)
        {
            if (_globalVolume.profile.TryGet(out Vignette profile))
                profile.active = value;
            else
                DebugManager.LogWarning("Vignette이 없습니다.");
        }

        public void SetVignetteColor(Color color)
        {
            if (_globalVolume.profile.TryGet(out Vignette profile))
                profile.color.value = color;
            else
                DebugManager.LogWarning("Vignette이 없습니다.");
        }

        public void SetVignette(
            float intensity = 0f,
            float smoothness = 0f,
            bool isRounded = false,
            Vector2? center = null,
            float duration = 0f,
            float boundsThreshold = 1f)
        {
            if (_globalVolume.profile.TryGet(out Vignette profile))
            {
                center ??= new Vector2(0.5f, 0.5f);
                profile.rounded.value = isRounded;

                if(_vignetteCoroutine != null) StopCoroutine(_vignetteCoroutine);

                if (duration == 0f)
                {
                    profile.intensity.value = intensity;
                    profile.smoothness.value = smoothness;
                    profile.center.value = center.Value;
                }
                else
                    _vignetteCoroutine = StartCoroutine(SetVignetteCoroutine(profile, intensity, smoothness, center.Value, duration , boundsThreshold));
            }
            else
                DebugManager.LogWarning("Vignette이 없습니다.");
        }

        private Coroutine _vignetteCoroutine;
        private IEnumerator SetVignetteCoroutine(
            Vignette profile,
            float intensity,
            float smoothness,
            Vector2 center,
            float duration,
            float boundsThreshold)
        {
            float timer = 0f;
            float originIntensity = profile.intensity.value;
            float originSmoothness = profile.smoothness.value;
            Vector2 originCenter = profile.center.value;
            float normalizeTime = 0f;
            boundsThreshold = boundsThreshold == 0f ? 0.5f : boundsThreshold;
            float normalizeBoundThreshold = Mathf.PI / (duration / boundsThreshold);
            while (timer < duration)
            {
                timer += Time.deltaTime;
                normalizeTime = Mathf.Abs(Mathf.Sin(normalizeBoundThreshold * timer));
                profile.intensity.value = Mathf.Lerp(originIntensity, intensity, normalizeTime);
                profile.smoothness.value = Mathf.Lerp(originSmoothness, smoothness, normalizeTime);
                profile.center.value = Vector2.Lerp(originCenter, center, normalizeTime);
                yield return null;
            }
            
            profile.intensity.value = originIntensity;
            profile.smoothness.value = originSmoothness;
            profile.center.value = originCenter;
        }
        #endregion
        
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

        private TweenerCore<Vector3, Vector3, VectorOptions> weaponMove;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> weaponRotate;
    
        [SerializeField] private float movement = 0.01f;
        [SerializeField] private float rotationAngle = 1;
        [SerializeField] private float time = 0.4f;
        
        private IEnumerator WeaponCameraShake()
        {
            var weaponCameraPosition = weaponCamera.transform.position;
            var weaponCameraRotation = weaponCamera.transform.rotation;
            DebugManager.Log($"{weaponCameraPosition}");
            
            //Ease.InOutCubic
            weaponMove = weaponCamera.transform.DOLocalMoveY(weaponCameraPosition.y + movement, time / 2).SetEase(Ease.Linear);
            weaponRotate = weaponCamera.transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(rotationAngle, 0, 0), time / 2);
            yield return new WaitForSeconds((time / 2) * 0.9f);
            while (true)
            {
                weaponMove?.Kill();
                weaponRotate?.Kill();
            
                weaponMove = weaponCamera.transform.DOLocalMoveY(weaponCameraPosition.y - movement, time).SetEase(Ease.Linear);
                weaponRotate = weaponCamera.transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(-rotationAngle, 0, 0), time);
                yield return new WaitForSeconds(time);
            
                weaponMove?.Kill();
                weaponRotate?.Kill();
            
                weaponMove = weaponCamera.transform.DOLocalMoveY(weaponCameraPosition.y + movement, time).SetEase(Ease.Linear);
                weaponRotate = weaponCamera.transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(rotationAngle, 0, 0), time);
                yield return new WaitForSeconds(time);
            }
        }
    }
}