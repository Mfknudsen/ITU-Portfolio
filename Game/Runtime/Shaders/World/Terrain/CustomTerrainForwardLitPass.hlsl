#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

sampler2D _Noise;
sampler2D _MainTex;
sampler2D _Control;
sampler2D _TerrainHolesTexture;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
sampler2D _Mask0, _Mask1, _Mask2, _Mask3;
float4 _MainTex_ST;
float4 _Control_ST;
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
float4 _Specular0, _Specular1, _Specular2, _Specular3;
float _NumLayersCount;
float _Metallic0, _Metallic1, _Metallic2, _Metallic3;
float _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3;
float _LayerHasMask0, _LayerHasMask1, _LayerHasMask2, _LayerHasMask3;
float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
float _Cascade0, _Cascade1, _Cascade2, _Cascade3;

float4 hash4(const float2 p)
{
    return frac(sin(float4(
            1.0 + dot(p, float2(37.0, 17.0)),
            2.0 + dot(p, float2(11.0, 47.0)),
            3.0 + dot(p, float2(41.0, 29.0)),
            4.0 + dot(p, float2(23.0, 31.0))))
        * 103.9);
}

float4 texture_no_tile(const float c, const sampler2D tex, const in float2 uv, const float dist)
{
    float4 v = float4(0.0, 0.0, 0.0, 0.0);
    float wt = 0.0;

    if (dist > _Cascade3)
    {
        v = tex2D(tex, uv);
        wt = 1;
    }

    if (dist < _Cascade3)
    {
        const float2 p = floor(uv);
        const float2 f = frac(uv);

        const float2 dx = ddx(uv);
        const float2 dy = ddy(uv);

        if (dist > _Cascade2)
        {
            for (int i = 0; i <= 1; i++)
            {
                const float2 g = float2(float(i), float(0));
                float4 o = hash4(p + g);
                const float2 r = g - f + o.xy;
                const float d = dot(r, r);
                const float w = exp(-5.0 * d);
                const float4 a_tex = tex2D(tex, uv + o.zw, dx, dy);
                v += w * a_tex;
                wt += w;
            }
        }
        else if (dist > _Cascade1)
        {
            for (int j = 0; j <= 1; j++)
                for (int i = 0; i <= 1; i++)
                {
                    if (abs(j) + abs(i) == 2)
                        continue;

                    const float2 g = float2(float(i), float(j));
                    float4 o = hash4(p + g);
                    const float2 r = g - f + o.xy;
                    const float d = dot(r, r);
                    const float w = exp(-5.0 * d);
                    const float4 a_tex = tex2D(tex, uv + o.zw, dx, dy);
                    v += w * a_tex;
                    wt += w;
                }
        }
        else
        {
            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    if (dist > _Cascade0 && abs(j) + abs(i) == 2)
                        continue;

                    const float2 g = float2(float(i), float(j));
                    float4 o = hash4(p + g);
                    const float2 r = g - f + o.xy;
                    const float d = dot(r, r);
                    const float w = exp(-5.0 * d);
                    const float4 a_tex = tex2D(tex, uv + o.zw, dx, dy);
                    v += w * a_tex;
                    wt += w;
                }
        }
    }

    return v / wt * c;
}

float3 texture_no_tile_normal(const sampler2D tex, const in float2 uv, const float normal_strength = 1,
                              const float dist = 0)
{
    float3 v = float3(0.0, 0.0, 0.0);
    float wt = 0.0;

    if (dist > _Cascade3)
    {
        v = UnpackNormalScale(tex2D(tex, uv), normal_strength);
        wt = 1;
    }

    if (dist < _Cascade3)
    {
        const float2 p = floor(uv);
        const float2 f = frac(uv);

        const float2 dx = ddx(uv);
        const float2 dy = ddy(uv);

        if (dist > _Cascade2)
        {
            for (int i = 0; i <= 1; i++)
            {
                const float2 g = float2(float(i), float(0));
                float4 o = hash4(p + g);
                const float2 r = g - f + o.xy;
                const float d = dot(r, r);
                const float w = exp(-5.0 * d);
                const float3 a_tex = UnpackNormalScale(tex2D(tex, uv + o.zw, dx, dy), normal_strength);
                v += w * a_tex;
                wt += w;
            }
        }
        else if (dist > _Cascade1)
        {
            for (int j = 0; j <= 1; j++)
                for (int i = 0; i <= 1; i++)
                {
                    if (abs(j) + abs(i) == 2)
                        continue;

                    const float2 g = float2(float(i), float(j));
                    float4 o = hash4(p + g);
                    const float2 r = g - f + o.xy;
                    const float d = dot(r, r);
                    const float w = exp(-5.0 * d);
                    const float3 a_tex = UnpackNormalScale(tex2D(tex, uv + o.zw, dx, dy), normal_strength);
                    v += w * a_tex;
                    wt += w;
                }
        }
        else
        {
            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    if (dist > _Cascade0 && abs(j) + abs(i) == 2)
                        continue;

                    const float2 g = float2(float(i), float(j));
                    float4 o = hash4(p + g);
                    const float2 r = g - f + o.xy;
                    const float d = dot(r, r);
                    const float w = exp(-5.0 * d);
                    const float3 a_tex = UnpackNormalScale(tex2D(tex, uv + o.zw, dx, dy), normal_strength);
                    v += w * a_tex;
                    wt += w;
                }
        }
    }

    return v / wt;
}

struct MeshData
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv_Control : TEXCOORD0;
    float2 uv_Splat0 : TEXCOORD1;
    float2 uv_Splat1 : TEXCOORD2;
    float2 uv_Splat2 : TEXCOORD3;
    float2 uv_Splat3 : TEXCOORD4;
};

struct Interpolators
{
    float2 uv_Control : TEXCOORD0;
    float2 uv_Splat0 : TEXCOORD1;
    float2 uv_Splat1 : TEXCOORD2;
    float2 uv_Splat2 : TEXCOORD3;
    float2 uv_Splat3 : TEXCOORD4;
    float4 position_cs : SV_POSITION;
    float3 normalWS : TEXCOORD5;
    float3 positionWS : TEXCOORD6;
    float4 tangentWS : TEXCOORD7;
};

Interpolators Vertex(MeshData input)
{
    Interpolators output;
    const VertexPositionInputs position_inputs = GetVertexPositionInputs(input.positionOS.xyz);
    const VertexNormalInputs normal_inputs = GetVertexNormalInputs(input.normalOS);

    output.position_cs = position_inputs.positionCS;
    output.positionWS = position_inputs.positionWS;

    output.uv_Control = TRANSFORM_TEX(input.uv_Control, _Control);

    output.uv_Splat0 = TRANSFORM_TEX(input.uv_Splat0, _Splat0);
    output.uv_Splat1 = TRANSFORM_TEX(input.uv_Splat1, _Splat1);
    output.uv_Splat2 = TRANSFORM_TEX(input.uv_Splat2, _Splat2);
    output.uv_Splat3 = TRANSFORM_TEX(input.uv_Splat3, _Splat3);

    output.normalWS = normal_inputs.normalWS;
    output.tangentWS = float4(normal_inputs.tangentWS, input.tangentOS.w);

    return output;
}

float4 Fragment(Interpolators i) : SV_Target
{
    float4 c = tex2D(_Control, i.uv_Control);

    #ifdef TERRAIN_SPLAT_ADDPASS
    const float4 weight = dot(tex2D(_Control, i.uv_Control), 1.0h);
    clip(weight <= 0.005h ? -1.0h : 1.0h);
    #endif

    clip(tex2D(_TerrainHolesTexture, i.uv_Control).r - .1);

    const float camera_distance = distance(_WorldSpaceCameraPos, i.positionWS);
    
    
    //Mask r = specular, g = occlusion, a = smoothness
    float4 mask0 = 0, mask1 = 0, mask2 = 0, mask3 = 0;
    if (_LayerHasMask0 == 0)
        mask0 = texture_no_tile(1, _Mask0, i.uv_Splat0, camera_distance);
    if (_LayerHasMask1 == 0)
        mask1 = texture_no_tile(1, _Mask1, i.uv_Splat1, camera_distance);
    if (_LayerHasMask2 == 0)
        mask2 = texture_no_tile(1, _Mask2, i.uv_Splat2, camera_distance);
    if (_LayerHasMask3 == 0)
        mask3 = texture_no_tile(1, _Mask3, i.uv_Splat3, camera_distance);

    InputData lightning_output = (InputData)0;
    SurfaceData surface_output = (SurfaceData)0;

    lightning_output.positionWS = i.positionWS;
    lightning_output.positionCS = i.position_cs;
    lightning_output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
    lightning_output.shadowCoord = TransformWorldToShadowCoord(i.positionWS);

    float4 col = surface_output.alpha;
    col += texture_no_tile(c.r, _Splat0, i.uv_Splat0, camera_distance);
    col += texture_no_tile(c.g, _Splat1, i.uv_Splat1, camera_distance);
    col += texture_no_tile(c.b, _Splat2, i.uv_Splat2, camera_distance);
    col += texture_no_tile(c.a, _Splat3, i.uv_Splat3, camera_distance);
    col *= 1.5;
    surface_output.albedo = col.rgb;
    surface_output.alpha = 1;

    float3 normal_ts = i.normalWS;
    normal_ts = lerp(normal_ts, texture_no_tile_normal(_Normal0, i.uv_Splat0, _NormalScale0, camera_distance).rgb,
                     c.r);
    normal_ts = lerp(normal_ts, texture_no_tile_normal(_Normal1, i.uv_Splat1, _NormalScale1, camera_distance).rgb,
                     c.g);
    normal_ts = lerp(normal_ts, texture_no_tile_normal(_Normal2, i.uv_Splat2, _NormalScale2, camera_distance).rgb,
                     c.b);
    normal_ts = lerp(normal_ts, texture_no_tile_normal(_Normal3, i.uv_Splat3, _NormalScale3, camera_distance).rgb,
                     c.a);
    const float3x3 tangent_to_world = CreateTangentToWorld(i.normalWS, i.tangentWS.xyz, i.tangentWS.w);
    surface_output.normalTS = TransformTangentToWorld(normal_ts, tangent_to_world);
    lightning_output.tangentToWorld = tangent_to_world;
    lightning_output.normalWS = TransformTangentToWorld(normal_ts, tangent_to_world);

    float smoothness = float(0);
    smoothness = lerp(smoothness, _Smoothness0 * (1 - _LayerHasMask0) + mask0.a * _LayerHasMask0, c.r);
    smoothness = lerp(smoothness, _Smoothness1 * (1 - _LayerHasMask1) + mask1.a * _LayerHasMask1, c.g);
    smoothness = lerp(smoothness, _Smoothness2 * (1 - _LayerHasMask2) + mask2.a * _LayerHasMask2, c.b);
    smoothness = lerp(smoothness, _Smoothness3 * (1 - _LayerHasMask3) + mask3.a * _LayerHasMask3, c.a);
    surface_output.smoothness = 1 - smoothness;

    float metallic = float(0);
    metallic = lerp(metallic, _Metallic0 * (1 - _LayerHasMask0) + mask0.r * _LayerHasMask0, c.r);
    metallic = lerp(metallic, _Metallic1 * (1 - _LayerHasMask1) + mask1.r * _LayerHasMask1, c.g);
    metallic = lerp(metallic, _Metallic2 * (1 - _LayerHasMask2) + mask2.r * _LayerHasMask2, c.b);
    metallic = lerp(metallic, _Metallic3 * (1 - _LayerHasMask3) + mask3.r * _LayerHasMask3, c.a);
    surface_output.metallic = metallic;

    float3 specular = float3(1, 1, 1);
    specular = lerp(specular, _Specular0.rgb, c.r);
    specular = lerp(specular, _Specular1.rgb, c.g);
    specular = lerp(specular, _Specular2.rgb, c.b);
    specular = lerp(specular, _Specular3.rgb, c.a);
    surface_output.specular = specular;

    float occlusion = float(0);
    if (_LayerHasMask0 == 0)
        occlusion = lerp(occlusion, mask0.g, c.r);
    if (_LayerHasMask1 == 0)
        occlusion = lerp(occlusion, mask1.g, c.g);
    if (_LayerHasMask2 == 0)
        occlusion = lerp(occlusion, mask2.g, c.b);
    if (_LayerHasMask3 == 0)
        occlusion = lerp(occlusion, mask3.g, c.a);
    surface_output.occlusion = occlusion;

    lightning_output.vertexLighting = unity_AmbientSky.rgb;

    return UniversalFragmentBlinnPhong(lightning_output, surface_output);
}
