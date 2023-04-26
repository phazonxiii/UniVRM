#ifndef VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include <UnityCG.cginc>
#endif

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_uv.hlsl"
#include "./vrmc_materials_mtoon_geometry_alpha.hlsl"
#include "./vrmc_materials_mtoon_geometry_normal.hlsl"
#include "./vrmc_materials_mtoon_lighting_unity.hlsl"
#include "./vrmc_materials_mtoon_lighting_mtoon.hlsl"

half4 MToonFragment(const FragmentInput fragmentInput) : SV_Target
{
    if (MToon_IsOutlinePass() && MToon_IsOutlineModeDisabled())
    {
        clip(-1);
    }

    const Varyings input = fragmentInput.varyings;
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Get MToon UV (with UVAnimation)
    const float2 uv = GetMToonGeometry_Uv(input.uv);

    // Get LitColor with Alpha
    #ifdef MTOON_URP
    const half4 litColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
    #else
    const half4 litColor = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;
    #endif

    // Alpha Test
    const half alpha = GetMToonGeometry_Alpha(litColor);

    // Get Normal
    const float3 normalWS = GetMToonGeometry_Normal(input, fragmentInput.facing, uv);

    // Get Unity Lighting
    const UnityLighting unityLighting = GetUnityLighting(input, normalWS);

    // Get MToon Lighting
    MToonInput mtoonInput;
    mtoonInput.uv = uv;
    mtoonInput.normalWS = normalWS;
    mtoonInput.viewDirWS = normalize(input.viewDirWS);
    mtoonInput.litColor = litColor.rgb;
    mtoonInput.alpha = alpha;
    half4 col = GetMToonLighting(unityLighting, mtoonInput);

    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        UnityLighting additionalUnityLighting = GetAdditionalUnityLighting(input, normalWS, lightIndex);
        col.rgb += GetMToonLighting(additionalUnityLighting, mtoonInput).rgb;
    }
    #endif

    // Apply Fog
    #ifdef MTOON_URP
    float fogCoord = input.fogFactorAndVertexLight.x;
    col.rgb = MixFog(col.rgb, fogCoord);
    #else
    UNITY_APPLY_FOG(fragmentInput.varyings.fogCoord, col);
    #endif

    return col;
}

#endif