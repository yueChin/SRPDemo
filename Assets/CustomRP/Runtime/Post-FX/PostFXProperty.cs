using UnityEngine;

namespace CustomRP
{
    public partial class PostFXStack
    {
        // public partial class Bloom
        // {
        //     
        // }
        private int m_BloomBicubicUpsamplingId = Shader.PropertyToID("_BloomBicubicUpsampling");
        private int m_BloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
        private int m_BloomThresholdId = Shader.PropertyToID("_BloomThreshold");
        private int m_BloomIntensityId = Shader.PropertyToID("_BloomIntensity");
        
        private int m_BloomPyramidId;
        
        private int m_BloomResultId = Shader.PropertyToID("_BloomResult");

        private int c_MaxBloomPyramidLevel = 16;

        private int m_ColorAdjustmentsId = Shader.PropertyToID("_ColorAdjustments");
        private int m_ColorFilterId = Shader.PropertyToID("_ColorFilter");

        private int m_WhiteBalanceId = Shader.PropertyToID("_WhiteBalance");

        private int m_SplitToningShadowsId = Shader.PropertyToID("_SplitToningShadows");
        private int m_SplitToningHightlightsId = Shader.PropertyToID("_SplitToningHighlights");

        private int m_ChannelMixerRedId = Shader.PropertyToID("_ChannelMixerRed");
        private int m_ChannelMixerGreenId = Shader.PropertyToID("_ChannelMixerGreen");
        private int m_ChannelMixerBlueId = Shader.PropertyToID("_ChannelMixerBlue");

        private int m_SMHShadowsId = Shader.PropertyToID("_SMHShadows");
        private int m_SMHMidtonesId = Shader.PropertyToID("_SMHMidtones");
        private int m_SMHHighlights = Shader.PropertyToID("_SMHHighlights");
        private int m_SMHRangeId = Shader.PropertyToID("_SMHRange");

        private int m_ColorGradingLUTId = Shader.PropertyToID("_ColorGradingLUT");
        private int m_ColorGradingLUTParametersId = Shader.PropertyToID("_ColorGradingLUTParameters");
        private int m_ColorGradingLUTInLogCId = Shader.PropertyToID("_ColorGradingLUTInLogC");

        private int m_FinalSrcBlendId = Shader.PropertyToID("_FinalSrcBlend");
        private int m_FinalDstBlendId = Shader.PropertyToID("_FinalDstBlend");
        private int m_ColorGradingResultId = Shader.PropertyToID("_ColorGradingResult");
        private int m_FinalResultId = Shader.PropertyToID("_FinalResultId");
        private int m_CopyBicubicId = Shader.PropertyToID("_CopyBicubic");

        private int m_FXAAConfigId = Shader.PropertyToID("_FXAAConfig");
        private const string c_FXAAQualityLowKeyword = "FXAA_QUALITY_LOW",c_FXAAQualityMediumKeyword = "FXAA_QUALITY_MEDIUM";
        enum Pass
        {
            BloomHorizontal,
            BloomVertical,
            BloomAdd,
            BloomScatter,
            BloomScatterFinal,
            BloomPrefilter,
            BloomPrefilterFireFlies,
            Copy,
            ColorGradingNone,
            ColorGradingASCS,
            ColorGradingNeutral,
            ColorGradingReinhard,
            ApplyColorGrading,
            ApplyColorGradingWithLuma,
            FinalRescale,
            FXAA,
            FXAAWithLuma,
        }
    }
}