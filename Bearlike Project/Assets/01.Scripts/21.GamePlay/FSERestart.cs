using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Util;

namespace GamePlay
{
    public class FSERestart : MonoBehaviour
    {
        [SerializeField] private UniversalRendererData rendererPipline;

        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        private static readonly int FullScreenIntensity = Shader.PropertyToID("_FullScreenIntensity");
        private static readonly int VignetteIntensity = Shader.PropertyToID("_VignetteIntensity");
        
        private void Start()
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
                            if (feature.name == "HitEffect")
                                FSPRF.passMaterial.SetFloat(Alpha, 0);
                            else if (feature.name == "FireEffect")
                            {
                        
                            }
                            else if (feature.name == "ShieldEffect")
                                FSPRF.passMaterial.SetFloat(FullScreenIntensity, 1.0f);
                            else if (feature.name == "HealEffect")
                                FSPRF.passMaterial.SetFloat(VignetteIntensity, 0);
                        }
                    }
                }
            }
        }
    }  
}

