// #ifndef CUSTOM_TEXTURE_INPUT_INCLUDED
// #define CUSTOM_TEXTURE_INPUT_INCLUDED
//
// TEXTURE2D(_DetailMap);
// SAMPLER(sampler_DetailMap);
//
// TEXTURE2D(_BaseMap);
// SAMPLER(sampler_BaseMap);
//
// TEXTURE2D(_MaskMap);
//
// float GetDetail(float2 detailUV)
// {
//     float4 map = SAMPLE_TEXTURE2D(_DetailMap,sampler_DetailMap,detailUV);
//     return map * 2.0 - 1.0;
// }
//
// float4 GetBase(float2 baseUV,float4 color ,float detailUV = 0.0)
// {
//     float4 map = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,baseUV);
//     //float4 color = INPUT_PROP(_BaseColor);
//     float4 detail = GetDetail(detailUV);
//     map += detail;
//     return map * color;
// }
//
// float4 GetMask(float2 baseUV)
// {
//     return SAMPLE_TEXTURE2D(_MaskMap,sampler_BaseMap,baseUV);
// }
//
// #endif