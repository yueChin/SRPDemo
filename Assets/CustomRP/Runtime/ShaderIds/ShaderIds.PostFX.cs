using UnityEngine;

public static partial class ShaderIds
{
    public static int FXSourceId = Shader.PropertyToID("_PostFXSource");
    public static int FXSource2Id = Shader.PropertyToID("_PostFXSource2");
    
    public static readonly int BloomBicubicUpsamplingId = Shader.PropertyToID("_BloomBicubicUpsampling");
    public static readonly int BloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
    public static readonly int BloomThresholdId = Shader.PropertyToID("_BloomThreshold");
    public static readonly int BloomIntensityId = Shader.PropertyToID("_BloomIntensity");

    public static readonly int BloomResultId = Shader.PropertyToID("_BloomResult");

    public static readonly int ColorAdjustmentsId = Shader.PropertyToID("_ColorAdjustments");
    public static readonly int ColorFilterId = Shader.PropertyToID("_ColorFilter");

    public static readonly int WhiteBalanceId = Shader.PropertyToID("_WhiteBalance");

    public static readonly int SplitToningShadowsId = Shader.PropertyToID("_SplitToningShadows");
    public static readonly int SplitToningHighlightsId = Shader.PropertyToID("_SplitToningHighlights");

    public static readonly int ChannelMixerRedId = Shader.PropertyToID("_ChannelMixerRed");
    public static readonly int ChannelMixerGreenId = Shader.PropertyToID("_ChannelMixerGreen");
    public static readonly int ChannelMixerBlueId = Shader.PropertyToID("_ChannelMixerBlue");

    public static readonly int SMHShadowsId = Shader.PropertyToID("_SMHShadows");
    public static readonly int SMHMidtonesId = Shader.PropertyToID("_SMHMidtones");
    public static readonly int SMHHighlights = Shader.PropertyToID("_SMHHighlights");
    public static readonly int SMHRangeId = Shader.PropertyToID("_SMHRange");

    public static readonly int ColorGradingLUTId = Shader.PropertyToID("_ColorGradingLUT");
    public static readonly int ColorGradingLUTParametersId = Shader.PropertyToID("_ColorGradingLUTParameters");
    public static readonly int ColorGradingLUTInLogCId = Shader.PropertyToID("_ColorGradingLUTInLogC");

    public static readonly int FinalSrcBlendId = Shader.PropertyToID("_FinalSrcBlend");
    public static readonly int FinalDstBlendId = Shader.PropertyToID("_FinalDstBlend");
    public static readonly int ColorGradingResultId = Shader.PropertyToID("_ColorGradingResult");
    public static readonly int FinalResultId = Shader.PropertyToID("_FinalResultId");
    public static readonly int CopyBicubicId = Shader.PropertyToID("_CopyBicubic");

    public static readonly int FXAAConfigId = Shader.PropertyToID("_FXAAConfig");

}