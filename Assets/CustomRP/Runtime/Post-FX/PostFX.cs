using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class PostFXStack
    {
        // public partial class Bloom
        // {
        //     
        // }
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
            m_Buffer.SetGlobalVector(m_BloomThresholdId,threshold);
            
            RenderTextureFormat format = m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            m_Buffer.GetTemporaryRT(m_BloomPrefilterId,width,height,0,FilterMode.Bilinear,format);
            Draw(sourceId,m_BloomPrefilterId,bloom.FadeFireflies ? Pass.BloomPrefilterFireFlies :Pass.BloomPrefilter);
            width /= 2;
            height /= 2;
            
            int fromId = m_BloomPrefilterId;
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
            
            m_Buffer.ReleaseTemporaryRT(m_BloomPrefilterId);
            m_Buffer.SetGlobalFloat(m_BloomBicubicUpsamplingId,bloom.BicubicUpsampling ? 1f : 0f);
            Pass combinePass,finalPass;
            float finalIntensity;
            if (bloom.Mode == Mode.Additive)
            {
                combinePass = finalPass= Pass.BloomAdd;
                m_Buffer.SetGlobalFloat(m_BloomIntensityId,1f);
                finalIntensity = bloom.Intensity;
            }
            else
            {
                combinePass = Pass.BloomScatter;
                finalPass = Pass.BloomScatterFinal;
                m_Buffer.SetGlobalFloat(m_BloomIntensityId,bloom.Scatter);
                finalIntensity = Mathf.Min(bloom.Intensity, 0.95f);
            }
            
            if (i > 1)
            {
                m_Buffer.ReleaseTemporaryRT(fromId - 1);
                toId -= 5;
                for (i -= 1; i > 0; i--)
                {
                    m_Buffer.SetGlobalTexture(m_FXSource2Id,toId + 1);
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
            m_Buffer.SetGlobalFloat(m_BloomIntensityId,finalIntensity);
            m_Buffer.SetGlobalTexture(m_FXSource2Id, sourceId);
            //Draw(fromId, BuiltinRenderTextureType.CameraTarget, combinePass);
            m_Buffer.GetTemporaryRT(m_BloomResultId, m_Camera.pixelWidth, m_Camera.pixelHeight,0,FilterMode.Bilinear,format);
            Draw(fromId,m_BloomResultId,finalPass);
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
            m_Buffer.GetTemporaryRT(m_ColorGradingLUTId,lutWeight,lutHeight,0,FilterMode.Bilinear,RenderTextureFormat.DefaultHDR);
            m_Buffer.SetGlobalVector(m_ColorGradingLUTParametersId,new Vector4(lutHeight, 0.5f / lutWeight, 0.5f / lutHeight,lutHeight / (lutHeight - 1f)));
            
            ToneMappingMode mode = m_PostFXSettings.ToneMapping.Mode;
            Pass pass =  Pass.ColorGradingNone + (int)mode;
            m_Buffer.SetGlobalFloat(m_ColorGradingLUTInLogCId,m_UseHDR && pass != Pass.ColorGradingNone ? 1f : 0f);
            Draw(sourceId, m_ColorGradingLUTId,pass);
            m_Buffer.SetGlobalVector(m_ColorGradingLUTParametersId,new Vector4( 1f / lutWeight, 1f / lutHeight,lutHeight - 1f));
        
            m_Buffer.SetGlobalFloat(m_FinalSrcBlendId,1f);
            m_Buffer.SetGlobalFloat(m_FinalDstBlendId,0f);
            if (m_AA.FXAA.Enable)
            {
                ConfigureFXAA();
                m_Buffer.GetTemporaryRT(m_ColorGradingResultId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,RenderTextureFormat.Default);
                Draw(sourceId,m_ColorGradingResultId, m_KeepAlpha ? Pass.ApplyColorGrading : Pass.ApplyColorGradingWithLuma);
            }
        
            if (m_BufferSize.x == m_Camera.pixelWidth)
            {
                if (m_AA.FXAA.Enable)
                {
                    DrawFinal(m_ColorGradingResultId, m_KeepAlpha ? Pass.FXAA: Pass.FXAAWithLuma);
                    m_Buffer.ReleaseTemporaryRT(m_ColorGradingResultId);
                }
                else
                {
                    DrawFinal(sourceId,Pass.ApplyColorGrading);
                }
            }
            else
            {
                m_Buffer.GetTemporaryRT(m_FinalResultId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,RenderTextureFormat.Default);
                if (m_AA.FXAA.Enable)
                {
                    Draw(m_ColorGradingResultId,m_FinalResultId, m_KeepAlpha ? Pass.FXAA: Pass.FXAAWithLuma);
                    m_Buffer.ReleaseTemporaryRT(m_ColorGradingResultId);
                }
                else
                {
                    Draw(sourceId,m_FinalResultId,Pass.ApplyColorGrading);
                }
        
                bool bicubicSampling = m_BicubicRescaling == BicubicRescalingMode.UpAndDown 
                                       || m_BicubicRescaling == BicubicRescalingMode.UpOnly && m_BufferSize.x < m_Camera.pixelWidth;
                m_Buffer.SetGlobalFloat(m_CopyBicubicId,bicubicSampling ? 1 : 0);
                DrawFinal(m_FinalResultId, Pass.FinalRescale);
                m_Buffer.ReleaseTemporaryRT(m_FinalResultId);
            }
            m_Buffer.ReleaseTemporaryRT(m_ColorGradingLUTId);
        }
        
        private void ConfigureColorAdjustments()
        {
            ColorAdjustmentsSettings colorAdjustmentsSettings = m_PostFXSettings.ColorAdjustmentsSettings;
            Vector4 v4 = new Vector4(Mathf.Pow(2f,colorAdjustmentsSettings.PostExposure)
                ,colorAdjustmentsSettings.Contrast * 0.01f + 1f
                ,colorAdjustmentsSettings.HueShift * (1f / 360f)
                ,colorAdjustmentsSettings.Saturation * 0.01f + 1f);
            m_Buffer.SetGlobalVector(m_ColorAdjustmentsId, v4);
            m_Buffer.SetGlobalColor(m_ColorFilterId,colorAdjustmentsSettings.ColorFilter.linear);
        }

        
        private void ConfigureWhiteBalance()
        {
            WhiteBalanceSettings whiteBalanceSettings = m_PostFXSettings.WhiteBalanceSettings;
            m_Buffer.SetGlobalVector(m_WhiteBalanceId,ColorUtils.ColorBalanceToLMSCoeffs(whiteBalanceSettings.Temperature,whiteBalanceSettings.Tint));
        }

        private void ConfigureSplitToning()
        {
            SplitToningSettings splitToningSettings = m_PostFXSettings.SplitToningSettings;
            Color splitColor = splitToningSettings.Shadows;
            splitColor.a = splitToningSettings.Balance * 0.01f;
            m_Buffer.SetGlobalColor(m_SplitToningShadowsId,splitColor);
            m_Buffer.SetGlobalColor(m_SplitToningHightlightsId,splitToningSettings.HightLights);
        }

        private void ConfigureChannelMixer()
        {
            ChannelMixerSettings channelMixerSettings = m_PostFXSettings.ChannelMixerSettings;
            m_Buffer.SetGlobalVector(m_ChannelMixerRedId,channelMixerSettings.Red);
            m_Buffer.SetGlobalVector(m_ChannelMixerGreenId,channelMixerSettings.Green);
            m_Buffer.SetGlobalVector(m_ChannelMixerBlueId, channelMixerSettings.Blue);
        }

        private void ConfigureShadowsMidTonesHighlights()
        {
            ShadowsMidtonesHighlightsSettings settings = m_PostFXSettings.ShadowsMidtonesHighlightsSettings;
            m_Buffer.SetGlobalColor(m_SMHShadowsId,settings.Shadows.linear);
            m_Buffer.SetGlobalColor(m_SMHMidtonesId, settings.Midtones.linear);
            m_Buffer.SetGlobalColor(m_SMHHighlights,settings.Highlights.linear);
            m_Buffer.SetGlobalVector(m_SMHRangeId,new Vector4(settings.ShadowStart,settings.ShadowsEnd,settings.HighlightsStart,settings.HighlightEnd));
        }

        private void ConfigureFXAA()
        {
            if (m_AA.FXAA.Quality == Quality.Low)
            {
                m_Buffer.EnableShaderKeyword(c_FXAAQualityLowKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityMediumKeyword);
            }
            else if (m_AA.FXAA.Quality == Quality.Medium)
            {
                m_Buffer.EnableShaderKeyword(c_FXAAQualityMediumKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityLowKeyword);
            }
            else
            {
                m_Buffer.DisableShaderKeyword(c_FXAAQualityLowKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityMediumKeyword);
            }
            m_Buffer.SetGlobalVector(m_FXAAConfigId,new Vector4(m_AA.FXAA.FixedThreshold,m_AA.FXAA.RelativeThreshold,m_AA.FXAA.SubpixelBlending));
        }

        private void ConfigureTAA()
        {
         
        }
    }
}