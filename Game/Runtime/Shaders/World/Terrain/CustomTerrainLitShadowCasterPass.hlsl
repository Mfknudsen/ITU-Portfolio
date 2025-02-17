#ifndef CUSTOM_TERRAIN_LIT_SHADOW_CASTER_PASS_INCLUDED
#define  CUSTOM_TERRAIN_LIT_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/Scripts/Runtime/Shaders/LitCommon.hlsl"

sampler2D _Control;
sampler2D _TerrainHolesTexture;
float4 _Control_ST;
float3 _LightDirection;

struct Attributes
{
    float3 position_os : POSITION;
    float3 normal_os : NORMAL;
    float2 uv : TEXCOORD0;
};

struct Interpolators
{
    float4 position_cs : SV_POSITION;
    float2 uv : TEXCOORD0;
};

float4 GetShadowCasterPositionCS(const float3 position_ws, const float3 normal_ws)
{
    const float3 light_direction_ws = _LightDirection;
    float4 position_cs = TransformWorldToHClip(ApplyShadowBias(position_ws, normal_ws, light_direction_ws));
    #if UNITY_REVERSED_Z
    position_cs.z = min(position_cs.z, UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif
    return position_cs;
}

Interpolators ShadowPassVertex(const Attributes input)
{
    Interpolators output;
    const VertexPositionInputs position_inputs = GetVertexPositionInputs(input.position_os);
    const VertexNormalInputs normal_inputs = GetVertexNormalInputs(input.normal_os);

    output.position_cs = GetShadowCasterPositionCS(position_inputs.positionWS, normal_inputs.normalWS);

    output.uv = TRANSFORM_TEX(input.uv, _Control);

    return output;
}

float4 ShadowPassFragment(Interpolators input) : SV_TARGET
{
    clip(tex2D(_TerrainHolesTexture, input.uv).r - .1);
    return 1;
}
#endif
