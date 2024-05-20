﻿#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED

// 필요한 HLSL 파일을 포함
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#ifndef SHADERGRAPH_PREVIEW
struct EdgeConstants
{
    float diffuse;
    float specular;
    float specularOffset;
    float distanceAttenuation;
    float shadowAttenuation;
    float rim;
    float rimOffset;
};

struct SurfaceVariables
{
    float3 normal;
    float3 view;
    float smoothness;
    float shininess;
    float rimThreshold;
    EdgeConstants ec;
};

float3 calculateCelShading(Light l, SurfaceVariables s)
{
    float shadowAttenuationSmoothstepped = smoothstep(0.0f, s.ec.shadowAttenuation, l.shadowAttenuation);
    float distanceAttenuationSmoothstepped = smoothstep(0.0f, s.ec.distanceAttenuation, l.distanceAttenuation);
    
    float attenuation = shadowAttenuationSmoothstepped * distanceAttenuationSmoothstepped;
    float diffuse = saturate(dot(s.normal, l.direction));
    diffuse *= attenuation;
    diffuse = diffuse > 0 ? 1 : 0;

    float3 h = SafeNormalize(l.direction + s.view);
    float specular = saturate(dot(s.normal, h));
    specular = pow(specular, s.shininess);
    specular *= diffuse * s.smoothness;
    specular = specular > 0.2 ? 1 : 0;

    float rim = 1 - dot(s.view ,s.normal);
    rim *= pow(diffuse, s.rimThreshold);
    rim = rim > 0.75 ? 1: 0;

    diffuse = smoothstep(0.0f, s.ec.diffuse, diffuse);
    specular = s.smoothness * smoothstep((1 - s.smoothness) * s.ec.specular + s.ec.specularOffset, s.ec.specular + s.ec.specularOffset, specular);
    rim = s.smoothness * smoothstep(s.ec.rim - 0.5f * s.ec.rimOffset, s.ec.rim + 0.5f * s.ec.rimOffset, rim);
    
    return l.color * (diffuse + max(specular, rim));
}

#endif

void LightingCelShaded_float(
    float Smoothness,float RimThreshold,float3 Position,float3 Normal,float3 View,
    float EdgeDiffuse, float EdgeSpecular, float EdgeSpecularOffset, float EdgeDistanceAttenuation, float EdgeShadowAttenuation, float EdgeRim, float EdgeRimOffset,
    
    out float3 Color)
{
    #if defined(SHADERGRAPH_PREVIEW)
    Color = float3(1.0f, 0.0f, 1.0f); // 예제 색상
    
    #else
    SurfaceVariables s;
    s.view = SafeNormalize(View);
    s.smoothness = Smoothness;
    s.normal = normalize(Normal);
    s.shininess = exp2(10 * Smoothness + 1);
    s.rimThreshold = RimThreshold;
    s.ec.diffuse = EdgeDiffuse;
    s.ec.specular = EdgeSpecular;
    s.ec.specularOffset = EdgeSpecularOffset;
    s.ec.distanceAttenuation = EdgeDistanceAttenuation;
    s.ec.shadowAttenuation = EdgeShadowAttenuation;
    s.ec.rim = EdgeRim;
    s.ec.rimOffset = EdgeRimOffset;
    
    #if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(Position);
    float4 shadowCoord = ComputeScreenPos(clipPos);

    #else
    float4 shadowCoord = TransformWorldToShadowCoord(Position);
    
    #endif
    
    Light light = GetMainLight(shadowCoord);
    Color = calculateCelShading(light, s);

    const int pixelLightCount = GetAdditionalLightsCount();
    for (int i =0; i < pixelLightCount; ++i)
    {
        light = GetAdditionalLight(i, Position, 1);
        Color += calculateCelShading(light,s);
    }
    
    #endif
}

#endif