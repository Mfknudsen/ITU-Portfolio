Shader "Unlit/CustomTerrain"
{
    Properties
    {
        _Cascade0 ("Distance cascade 0", Float) = 150
        _Cascade1 ("Distance cascade 1", Float) = 200
        _Cascade2 ("Distance cascade 2", Float) = 250
        _Cascade3 ("Distance cascade 3", Float) = 300

        [HideInInspector] [PerRendererData] _NumLayersCount ("Total Layer Count", Float) = 1.0

        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}

        [HideInInspector] _TerrainHolesTexture ("Holes", 2D) = "grey" {}

        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "grey" {}

        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}

        [HideInInspector] _Smoothness0 ("Smoothness 0 (R)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness1 ("Smoothness 1 (G)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness2 ("Smoothness 2 (B)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness3 ("Smoothness 3 (A)", Range(0.0, 1.0)) = 0.0

        [HideInInspector][Gamma] _Metallic0 ("Metallic 0 (R)", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic1 ("Metallic 1 (G)", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic2 ("Metallic 2 (B)", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic3 ("Metallic 3 (A)", Range(0.0, 1.0)) = 0.0

        [HideInInspector] _Mask0 ("Mask 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Mask1 ("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask2 ("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask3 ("Mask 3 (A)", 2D) = "grey" {}

        [HideInInspector] _LayerHasMask0 ("Layer Has Mask 0 (R)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _LayerHasMask1 ("Layer Has Mask 1 (G)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _LayerHasMask2 ("Layer Has Mask 2 (B)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _LayerHasMask3 ("Layer Has Mask 3 (A)", Range(0.0, 1.0)) = 0.0

        [HideInInspector] _NormalScale0 ("Layer Has Mask 0 (R)", Float) = 1.0
        [HideInInspector] _NormalScale1 ("Layer Has Mask 1 (G)", Float) = 1.0
        [HideInInspector] _NormalScale2 ("Layer Has Mask 2 (B)", Float) = 1.0
        [HideInInspector] _NormalScale3 ("Layer Has Mask 3 (A)", Float) = 1.0

        [HideInInspector] _Specular0 ("Specular 0", Color) = (1, 1, 1, 1)
        [HideInInspector] _Specular1 ("Specular 1", Color) = (1, 1, 1, 1)
        [HideInInspector] _Specular2 ("Specular 2", Color) = (1, 1, 1, 1)
        [HideInInspector] _Specular3 ("Specular 3", Color) = (1, 1, 1, 1)
    }

    HLSLINCLUDE
    #pragma multi_compile_fragment __ _ALPHATEST_ON
    ENDHLSL

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry-100"
            "RenderType"="Opague"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector"="True"
            "TerrainCompatible"="True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #define _SPECULAR_COLOR
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma shader_feature_local_fragment _TERRAIN_BLEND_HEIGHT
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _MASKMAP
            // Sample normal in pixel shader when doing instancing
            #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Assets/Scripts/Runtime/Shaders/World/Terrain/CustomTerrainForwardLitPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment decal:add

            #include "Assets/Scripts/Runtime/Shaders/World/Terrain/CustomTerrainLitShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    CustomEditor "CustomTerrainGUI"

    Dependency "AddPassShader" = "Hidden/Lit/CustomTerrainAddPass"

    Fallback "Universal Render Pipeline/Terrain/Lit"
}