using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private readonly int m_BloomPyramidId;
        
        private int c_MaxBloomPyramidLevel = 16;

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
        
        private int m_FrameCount = 0;
        private Vector2 m_JitterVector2;
        bool m_ResetHistory = true;
        private RenderTexture[] m_HistoryTextures = new RenderTexture[2];
        private RenderTargetIdentifier[] m_TargetIdentifiers;
        //长度为8的Halton序列
        private Vector2[] m_HaltonSequences = new Vector2[]
        {
            new Vector2(0.5f, 1.0f / 3),
            new Vector2(0.25f, 2.0f / 3),
            new Vector2(0.75f, 1.0f / 9),
            new Vector2(0.125f, 4.0f / 9),
            new Vector2(0.625f, 7.0f / 9),
            new Vector2(0.375f, 2.0f / 9),
            new Vector2(0.875f, 5.0f / 9),
            new Vector2(0.0625f, 8.0f / 9),
        };
    }
}