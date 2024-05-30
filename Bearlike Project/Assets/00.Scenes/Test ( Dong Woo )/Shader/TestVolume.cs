using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DebugManager = Manager.DebugManager;

namespace Test
{
    public class TestVolume : MonoBehaviour
    {
        public Volume _globalVolume;

        public float duration = 1f;

        public float vignetteIntensity;
        public float vignetteSmoothness;
        public float vignetteBoundsThreshold;

        public void ScreenImpact()
        {
            SetChromaticAberration();
            SetVignette();
            
            SetChromaticAberration(1f, duration);
            SetVignette(vignetteIntensity, vignetteSmoothness, false, null, duration, vignetteBoundsThreshold);
        }
        
        #region Chromatic Aberration Function

        public void SetActiveChromaticAberration(bool value)
        {
            if (_globalVolume.profile.TryGet(out ChromaticAberration profile))
                profile.active = value;
            else
                DebugManager.LogWarning("Chromatic Aberration이 없습니다.");
        }

        public void SetChromaticAberration(float intensity = 0f, float duration = 0f)
        {
            if (_globalVolume.profile.TryGet(out ChromaticAberration profile))
            {
                if(duration == 0f)
                    profile.intensity.value = intensity;
                else
                {
                    if(_chromaticAberrationCoroutine != null) StopCoroutine(_chromaticAberrationCoroutine);
                    _chromaticAberrationCoroutine = StartCoroutine(SetChromaticAberrationCoroutine(profile, intensity, duration));
                }
            }
            else
                DebugManager.LogWarning("Chromatic Aberration이 없습니다.");
        }

        private Coroutine _chromaticAberrationCoroutine;
        private IEnumerator SetChromaticAberrationCoroutine(ChromaticAberration profile, float intensity, float duration)
        {
            float timer = 0f;
            float originIntensity = profile.intensity.value;
            float normalizeTime = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                normalizeTime = timer / duration;
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
                
                if(duration == 0f)
                    profile.intensity.value = intensity;
                else
                {
                    if(_vignetteCoroutine != null) StopCoroutine(_vignetteCoroutine);
                    _vignetteCoroutine = StartCoroutine(SetVignetteCoroutine(profile, intensity, smoothness, center.Value, duration , boundsThreshold));
                }
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
    }
}
