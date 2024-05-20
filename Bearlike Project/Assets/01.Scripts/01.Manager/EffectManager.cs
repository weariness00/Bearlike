using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EffectManager : MonoBehaviour
{
    public List<Material> effectList;

    public FullScreenPassRendererFeature FullScreenPassRendererFeature;
    
    void Start()
    {
        // 현재 카메라의 UniversalAdditionalCameraData를 가져옵니다.
        var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
    
        // 현재 Renderer를 가져옵니다.
        var renderer = cameraData.scriptableRenderer;
            
        var featuresField = typeof(ScriptableRenderer).GetField("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

        if (featuresField != null)
        {
            var features = featuresField.GetValue(renderer) as List<ScriptableRendererFeature>;
            if (features != null)
            {
                foreach (var feature in features)
                {
                    if (feature is FullScreenPassRendererFeature)
                    {
                        FullScreenPassRendererFeature = feature as FullScreenPassRendererFeature;
                        break;
                    }
                }
            }
        }
    
        // FullScreenPassRendererFeature의 활성화 여부를 설정합니다.
        if (FullScreenPassRendererFeature != null)
        {
            FullScreenPassRendererFeature.SetActive(true);
        }
        else
        {
            Debug.LogWarning("FullScreenPassRendererFeature를 찾을 수 없습니다.");
        }
    }
     
    void Update()
    {
        // 필요 시 스크립트에서 FullScreen Pass의 활성화 여부를 변경할 수 있습니다.
        if (FullScreenPassRendererFeature != null)
        {
            FullScreenPassRendererFeature.SetActive(true);
        }
    }
}
