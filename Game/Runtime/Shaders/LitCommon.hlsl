#ifndef LIT_COMMON_INCLUDED
#define LIT_COMMON_INCLUDED

#include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
void TestAlphaClip(float colorSample, float cutOff = .1)
{
    #ifdef _ALPHA_CUTOUT
    clip(colorSample - cutOff);
    #endif
}
#endif