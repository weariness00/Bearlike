using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class URPRendererFeaturesManager : MonoBehaviour
{
    // public List<Material> effectList;

    private Dictionary<string, FullScreenPassRendererFeature> _rendererFeaturesEffectDictionary = new Dictionary<string, FullScreenPassRendererFeature>();
    [SerializeField] private List<string> _rendererFeaturesEffectNames = new List<string>();
    
    void Start()
    {
        var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
    
        var renderer = cameraData.scriptableRenderer;
            
        var featuresField = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

        if (featuresField != null)
        {
            var features = featuresField.GetValue(renderer) as List<ScriptableRendererFeature>;
            if (features != null)
            {
                foreach (var feature in features)
                {
                    if (feature is FullScreenPassRendererFeature FSPRF)
                    {
                        _rendererFeaturesEffectDictionary.Add(feature.name, FSPRF);
                        // if(feature.name == "FireEffect")
                        //     fireRendererFeaturesEffect = feature as FullScreenPassRendererFeature;
                        // else if(feature.name == "ShieldEffect")
                        //     ShieldRendererFeaturesEffect = feature as FullScreenPassRendererFeature;
                        // else if (feature.name == "HitEffect")
                        //     HitRendererFeaturesEffect = feature as FullScreenPassRendererFeature;
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
     
    void Update()
    {
        
    }
    
    // coroutine으로 알파값 조정해서 알아서 이펙트가 실행되는 함수 만들기 
}
