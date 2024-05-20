#ifndef LIGHTING_CAl_SHADED_INCLUDED
#define LIGHTING_CAl_SHADED_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void LightDirection_Cal_Dot(float3 Position, float3 Normal, out float Out)
{
    UniversalLight light = GetMainLight();
    float d = dot(Normal, light.direction);

    const int pixelLightCount = GetAdditionalLightsCount();
    for (int i =0; i < pixelLightCount; ++i)
    {
        light = GetAdditionalLight(i, Position, 1);
        d += dot(Normal, light.direction);
    }

    Out = d;
}

#endif