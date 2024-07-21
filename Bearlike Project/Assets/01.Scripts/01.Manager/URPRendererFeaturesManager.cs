using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Util;

namespace Manager
{
    public class URPRendererFeaturesManager : Singleton<URPRendererFeaturesManager>
    {
        [SerializeField] private UniversalRendererData rendererPipline;
        private Dictionary<string, FullScreenPassRendererFeature> _rendererFeaturesEffectDictionary = new Dictionary<string, FullScreenPassRendererFeature>();
        [SerializeField] private List<string> _rendererFeaturesEffectNames = new List<string>();
        
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        private static readonly int FullScreenIntensity = Shader.PropertyToID("_FullScreenIntensity");
        private static readonly int VignetteIntensity = Shader.PropertyToID("_VignetteIntensity");
        
        #region Unity Evnet Function
        
        void Start()
        {
            var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
    
            var renderer = cameraData.scriptableRenderer;
            
            var featuresField = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            if (featuresField != null)
            {
                
                // var features = featuresField.GetValue(rendererPipline) as List<ScriptableRendererFeature>;
                var features = rendererPipline.rendererFeatures;
                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        if (feature is FullScreenPassRendererFeature FSPRF)
                        {
                            _rendererFeaturesEffectDictionary.Add(feature.name, FSPRF);
                        }
                    }
                }
            }

            foreach (var name in _rendererFeaturesEffectNames)
            {
                if (_rendererFeaturesEffectDictionary.TryGetValue(name, out var effect))
                {
                    effect.SetActive(false);
                }
            }
        }
        
        #endregion

        public void StartEffect(string name)
        {
            StartCoroutine(PlayEffectCoroutine(name));
        }
        
        IEnumerator PlayEffectCoroutine(string name)
        {
            FullScreenPassRendererFeature fullScreenPassRendererFeature;
        
            if (_rendererFeaturesEffectDictionary.TryGetValue(name, out fullScreenPassRendererFeature))
            {
                fullScreenPassRendererFeature.SetActive(true);
            }
            
            for (int index = 0; index < _rendererFeaturesEffectNames.Count; ++index)
            {
                if (name == _rendererFeaturesEffectNames[index])
                {
                    StartCoroutine(EffectAlphaCoroutine(index, fullScreenPassRendererFeature.passMaterial));
                }
            }
        
            yield return new WaitForSeconds(0.5f);
            // // 굳이 if문 필요없다.
            // if (_rendererFeaturesEffectDictionary.TryGetValue(name, out fullScreenPassRendererFeature))
            // {
            //     fullScreenPassRendererFeature.SetActive(false);
            // }
        }

        IEnumerator EffectAlphaCoroutine(int mode, Material material)
        {
            if (mode == 0)
            {
                float value = 0.6f;
                while (value <= 1.0f)
                {
                    material.SetFloat(Alpha, value);
                    yield return new WaitForSeconds(0.0025f);
                    value += 0.01f;
                }

                yield return new WaitForSeconds(0.5f);
                
                while (value >= 0.0f)
                {
                    material.SetFloat(Alpha, value);
                    yield return new WaitForSeconds(0.0025f);
                    value -= 0.01f;
                }
                material.SetFloat(Alpha, 0);
            }
            else if (mode == 3)
            {
                float value = 0.7f;
                while (value <= 0.9f)
                {
                    material.SetFloat(VignetteIntensity, value);
                    yield return new WaitForSeconds(0.0025f);
                    value += 0.01f;
                }

                yield return new WaitForSeconds(0.5f);
                
                while (value >= 0.0f)
                {
                    material.SetFloat(VignetteIntensity, value);
                    yield return new WaitForSeconds(0.0025f);
                    value -= 0.01f;
                }
                material.SetFloat(VignetteIntensity, 0);
            }
            // else if (1 <= mode && mode <= 2)
            // {
            //     DebugManager.ToDo("FullScreenEffect의 fire와 shield의 시간은 나중에 연동하자"); 
            //     float value = 1.0f;
            //     while (value >= 0.95f)
            //     {
            //         material.SetFloat(FullScreenIntensity, value);
            //         yield return new WaitForSeconds(0.1f);
            //         value -= 0.01f;
            //     }
            //
            //     while (value <= 1.0f)
            //     {
            //         material.SetFloat(FullScreenIntensity, value);
            //         yield return new WaitForSeconds(0.1f);
            //         value -= 0.01f;
            //     }
            // }
        }
    
    }
}
