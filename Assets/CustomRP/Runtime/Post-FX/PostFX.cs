using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace CustomRP
{
    public partial class PostFXStack
    {
        
        private readonly int m_BloomPyramidId;
        
        private int c_MaxBloomPyramidLevel = 16;

        private enum Pass
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
            TAA,
        }
        
        private bool DoBloom(int sourceId)
        {
            BloomSettings bloom = m_PostFXSettings.Bloom;
            int width, height;
            if (bloom.IgnoreRenderScale)
            {
                width = m_Camera.pixelWidth / 2;
                height = m_Camera.pixelHeight / 2;
            }
            else
            {
                width = m_BufferSize.x / 2;
                height = m_BufferSize.y / 2;
            }
            if (bloom.MaxIterations == 0 || bloom.Intensity <= 0f || height < bloom.DownScaleLimit * 2 || width < bloom.DownScaleLimit * 2)
            {
                return false;
            }
        
            m_Buffer.BeginSample("Bloom");
        
            Vector4 threshold;
            threshold.x = Mathf.GammaToLinearSpace(bloom.Threshold);
            threshold.y = threshold.x * bloom.ThresholdKnee;
            threshold.z = 2f * threshold.y;
            threshold.w = 0.25f / (threshold.y + 0.00001f);
            threshold.y -= threshold.x;
            m_Buffer.SetGlobalVector(ShaderIds.BloomThresholdId,threshold);
            
            RenderTextureFormat format = m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            m_Buffer.GetTemporaryRT(ShaderIds.BloomPrefilterId,width,height,0,FilterMode.Bilinear,format);
            Draw(sourceId,ShaderIds.BloomPrefilterId,bloom.FadeFireflies ? Pass.BloomPrefilterFireFlies :Pass.BloomPrefilter);
            width /= 2;
            height /= 2;
            
            int fromId = ShaderIds.BloomPrefilterId;
            int toId = m_BloomPyramidId + 1;
            int i;
            for (i = 0; i < bloom.MaxIterations; i++)
            {
                if (height < bloom.DownScaleLimit || width < bloom.DownScaleLimit)
                {
                    break;
                }
        
                int midId = toId - 1;
                m_Buffer.GetTemporaryRT(midId,width,height,0,FilterMode.Bilinear,format);
                m_Buffer.GetTemporaryRT(toId,width,height,0,FilterMode.Bilinear,format);
                Draw(fromId,midId,Pass.BloomHorizontal);
                Draw(midId,toId,Pass.BloomVertical);
                fromId = toId;
                toId += 2;
                width /= 2;
                height /= 2;
            }
            
            m_Buffer.ReleaseTemporaryRT(ShaderIds.BloomPrefilterId);
            m_Buffer.SetGlobalFloat(ShaderIds.BloomBicubicUpsamplingId,bloom.BicubicUpsampling ? 1f : 0f);
            Pass combinePass,finalPass;
            float finalIntensity;
            if (bloom.Mode == Mode.Additive)
            {
                combinePass = finalPass= Pass.BloomAdd;
                m_Buffer.SetGlobalFloat(ShaderIds.BloomIntensityId,1f);
                finalIntensity = bloom.Intensity;
            }
            else
            {
                combinePass = Pass.BloomScatter;
                finalPass = Pass.BloomScatterFinal;
                m_Buffer.SetGlobalFloat(ShaderIds.BloomIntensityId,bloom.Scatter);
                finalIntensity = Mathf.Min(bloom.Intensity, 0.95f);
            }
            
            if (i > 1)
            {
                m_Buffer.ReleaseTemporaryRT(fromId - 1);
                toId -= 5;
                for (i -= 1; i > 0; i--)
                {
                    m_Buffer.SetGlobalTexture(ShaderIds.FXSource2Id,toId + 1);
                    Draw(fromId,toId,combinePass);
                    m_Buffer.ReleaseTemporaryRT(fromId);
                    m_Buffer.ReleaseTemporaryRT(toId + 1);
                    fromId = toId;
                    toId -= 2;
                }
            }
            else
            {
                m_Buffer.ReleaseTemporaryRT(m_BloomPyramidId);
            }
            
            //Draw(formId,BuiltinRenderTextureType.CameraTarget,Pass.BloomHorizontal);
            m_Buffer.SetGlobalFloat(ShaderIds.BloomIntensityId,finalIntensity);
            m_Buffer.SetGlobalTexture(ShaderIds.FXSource2Id, sourceId);
            //Draw(fromId, BuiltinRenderTextureType.CameraTarget, combinePass);
            m_Buffer.GetTemporaryRT(ShaderIds.BloomResultId, m_Camera.pixelWidth, m_Camera.pixelHeight,0,FilterMode.Bilinear,format);
            Draw(fromId,ShaderIds.BloomResultId,finalPass);
            m_Buffer.ReleaseTemporaryRT(fromId);
            m_Buffer.EndSample("Bloom");
            return true;
        }
        
        private void DoFinal(int sourceId)
        {
            ConfigureColorAdjustments();
            ConfigureWhiteBalance();
            ConfigureSplitToning();
            ConfigureChannelMixer();
            ConfigureShadowsMidTonesHighlights();
            
            int lutHeight = m_ColorLUTResolution;
            int lutWeight = lutHeight * lutHeight;
            m_Buffer.GetTemporaryRT(ShaderIds.ColorGradingLUTId,lutWeight,lutHeight,0,FilterMode.Bilinear,RenderTextureFormat.DefaultHDR);
            m_Buffer.SetGlobalVector(ShaderIds.ColorGradingLUTParametersId,new Vector4(lutHeight, 0.5f / lutWeight, 0.5f / lutHeight,lutHeight / (lutHeight - 1f)));
            
            ToneMappingMode mode = m_PostFXSettings.ToneMapping.Mode;
            Pass pass =  Pass.ColorGradingNone + (int)mode;
            m_Buffer.SetGlobalFloat(ShaderIds.ColorGradingLUTInLogCId,m_UseHDR && pass != Pass.ColorGradingNone ? 1f : 0f);
            Draw(sourceId, ShaderIds.ColorGradingLUTId,pass);
            m_Buffer.SetGlobalVector(ShaderIds.ColorGradingLUTParametersId,new Vector4( 1f / lutWeight, 1f / lutHeight,lutHeight - 1f));

            m_Buffer.SetGlobalFloat(ShaderIds.FinalSrcBlendId,1f);
            m_Buffer.SetGlobalFloat(ShaderIds.FinalDstBlendId,0f);
            if (m_AA.FXAA.Enable || m_AA.TAA.Enable)
            {
                if (m_AA.FXAA.Enable)
                {
                    ConfigureFXAA();
                }
                else if(m_AA.TAA.Enable)
                {
                    ConfigureTAA(ref m_CameraProperty.HistoryTextures,ref m_CameraProperty.HistoryMotionVectorTextures);
                }
                m_Buffer.GetTemporaryRT(ShaderIds.ColorGradingResultId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,RenderTextureFormat.Default);
                Draw(sourceId,ShaderIds.ColorGradingResultId, m_KeepAlpha ? Pass.ApplyColorGrading : Pass.ApplyColorGradingWithLuma);
            }
        
            if (m_BufferSize.x == m_Camera.pixelWidth)
            {
                if (m_AA.FXAA.Enable)
                {
                    DrawFinal(ShaderIds.ColorGradingResultId, m_KeepAlpha ? Pass.FXAA: Pass.FXAAWithLuma);
                    m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorGradingResultId);
                }
                else if (m_AA.TAA.Enable)
                {
                    m_Buffer.GetTemporaryRT(ShaderIds.FinalResultId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
                    Draw(ShaderIds.ColorGradingResultId,ShaderIds.FinalResultId, Pass.TAA);
                    AfterDrawTAA(ref m_CameraProperty.HistoryTextures,ref m_CameraProperty.HistoryMotionVectorTextures);
                    m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorGradingResultId);
                    
                    bool bicubicSampling = m_BicubicRescaling == BicubicRescalingMode.UpAndDown 
                                           || m_BicubicRescaling == BicubicRescalingMode.UpOnly && m_BufferSize.x < m_Camera.pixelWidth;
                    m_Buffer.SetGlobalFloat(ShaderIds.CopyBicubicId,bicubicSampling ? 1 : 0);
                    DrawFinal(ShaderIds.FinalResultId, Pass.FinalRescale);
                    m_Buffer.ReleaseTemporaryRT(ShaderIds.FinalResultId);
                }
                else
                {
                    DrawFinal(sourceId,Pass.ApplyColorGrading);
                }
            }
            else
            {
                m_Buffer.GetTemporaryRT(ShaderIds.FinalResultId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
                if (m_AA.FXAA.Enable)
                {
                    Draw(ShaderIds.ColorGradingResultId,ShaderIds.FinalResultId, m_KeepAlpha ? Pass.FXAA: Pass.FXAAWithLuma);
                    m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorGradingResultId);
                }
                else if (m_AA.TAA.Enable)
                {
                    Draw(ShaderIds.ColorGradingResultId,ShaderIds.FinalResultId, Pass.TAA);
                    AfterDrawTAA(ref m_CameraProperty.HistoryTextures,ref m_CameraProperty.HistoryMotionVectorTextures);
                    m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorGradingResultId);
                }
                else
                {
                    Draw(sourceId,ShaderIds.FinalResultId,Pass.ApplyColorGrading);
                }
        
                bool bicubicSampling = m_BicubicRescaling == BicubicRescalingMode.UpAndDown 
                                       || m_BicubicRescaling == BicubicRescalingMode.UpOnly && m_BufferSize.x < m_Camera.pixelWidth;
                m_Buffer.SetGlobalFloat(ShaderIds.CopyBicubicId,bicubicSampling ? 1 : 0);
                DrawFinal(ShaderIds.FinalResultId, Pass.FinalRescale);
                m_Buffer.ReleaseTemporaryRT(ShaderIds.FinalResultId);
            }
            m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorGradingLUTId);
        }
        
        private void ConfigureColorAdjustments()
        {
            ColorAdjustmentsSettings colorAdjustmentsSettings = m_PostFXSettings.ColorAdjustmentsSettings;
            Vector4 v4 = new Vector4(Mathf.Pow(2f,colorAdjustmentsSettings.PostExposure)
                ,colorAdjustmentsSettings.Contrast * 0.01f + 1f
                ,colorAdjustmentsSettings.HueShift * (1f / 360f)
                ,colorAdjustmentsSettings.Saturation * 0.01f + 1f);
            m_Buffer.SetGlobalVector(ShaderIds.ColorAdjustmentsId, v4);
            m_Buffer.SetGlobalColor(ShaderIds.ColorFilterId,colorAdjustmentsSettings.ColorFilter.linear);
        }

        
        private void ConfigureWhiteBalance()
        {
            WhiteBalanceSettings whiteBalanceSettings = m_PostFXSettings.WhiteBalanceSettings;
            m_Buffer.SetGlobalVector(ShaderIds.WhiteBalanceId,ColorUtils.ColorBalanceToLMSCoeffs(whiteBalanceSettings.Temperature,whiteBalanceSettings.Tint));
        }

        private void ConfigureSplitToning()
        {
            SplitToningSettings splitToningSettings = m_PostFXSettings.SplitToningSettings;
            Color splitColor = splitToningSettings.Shadows;
            splitColor.a = splitToningSettings.Balance * 0.01f;
            m_Buffer.SetGlobalColor(ShaderIds.SplitToningShadowsId,splitColor);
            m_Buffer.SetGlobalColor(ShaderIds.SplitToningHighlightsId,splitToningSettings.HightLights);
        }

        private void ConfigureChannelMixer()
        {
            ChannelMixerSettings channelMixerSettings = m_PostFXSettings.ChannelMixerSettings;
            m_Buffer.SetGlobalVector(ShaderIds.ChannelMixerRedId,channelMixerSettings.Red);
            m_Buffer.SetGlobalVector(ShaderIds.ChannelMixerGreenId,channelMixerSettings.Green);
            m_Buffer.SetGlobalVector(ShaderIds.ChannelMixerBlueId, channelMixerSettings.Blue);
        }

        private void ConfigureShadowsMidTonesHighlights()
        {
            ShadowsMidtonesHighlightsSettings settings = m_PostFXSettings.ShadowsMidtonesHighlightsSettings;
            m_Buffer.SetGlobalColor(ShaderIds.SMHShadowsId,settings.Shadows.linear);
            m_Buffer.SetGlobalColor(ShaderIds.SMHMidtonesId, settings.Midtones.linear);
            m_Buffer.SetGlobalColor(ShaderIds.SMHHighlights,settings.Highlights.linear);
            m_Buffer.SetGlobalVector(ShaderIds.SMHRangeId,new Vector4(settings.ShadowStart,settings.ShadowsEnd,settings.HighlightsStart,settings.HighlightEnd));
        }

    }
}