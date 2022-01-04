using CustomRP;
using CustomRP.CameraRender;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{
    public sealed class HistoryTexture :IDisposeProperty
    {
        public RenderTexture HistoryRT;
        public RenderTexture HistoryMotionVectorRT;
        public Vector2 Jitter;    
        
        public void SetupHistoryTexture(Vector2Int buffSize,bool useHDR)
        {
            if (HistoryRT == null)
            {
                HistoryRT = new RenderTexture(buffSize.x, buffSize.y, 0,useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default, 0)
                {
                    name = "HistoryRT",
                    bindTextureMS = false
                };
                HistoryRT.Create();
            }


            if (HistoryMotionVectorRT == null)
            {
                HistoryMotionVectorRT = new RenderTexture(buffSize.x, buffSize.y, 0, RenderTextureFormat.Depth, 0)
                {
                    name = "HistoryMotionVectorRT",
                    bindTextureMS = false
                };
                HistoryMotionVectorRT.Create();
            }
        }
        
        public void SetupProperty(Vector2Int buffSize,bool useHDR)
        {
            int bufferWidth = buffSize.x;
            int buffHeight = buffSize.y;
            if (!HistoryRT)
            {
                HistoryRT = new RenderTexture(bufferWidth, buffHeight, 0, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default, 0)
                {
                    bindTextureMS = false
                };
                HistoryRT.Create();
            }

            if (!HistoryMotionVectorRT)
            {
                HistoryMotionVectorRT = new RenderTexture(bufferWidth,buffHeight, 0, RenderTextureFormat.Depth, 0)
                {
                    bindTextureMS = false
                };
                HistoryMotionVectorRT.Create();
            }
            CameraProperty.Resize(HistoryRT, bufferWidth, buffHeight);
            CameraProperty.Resize(HistoryMotionVectorRT, bufferWidth, buffHeight);
        }

        public void SetHistory(Vector2Int buffSize, CommandBuffer buffer, ref RenderTexture history, RenderTargetIdentifier renderTarget)
        {
            if (history == null)
            {
                history = new RenderTexture(buffSize.x, buffSize.y, 0, RenderTextureFormat.Default, 0)
                {
                    filterMode = FilterMode.Bilinear,
                    bindTextureMS = false,
                    antiAliasing = 1
                };
                buffer.CopyTexture(renderTarget, history);
            }
            else if (history.width != buffSize.x || history.height != buffSize.y)
            {
                history.Release();
                CoreUtils.Destroy(history);
                history = new RenderTexture(buffSize.x, buffSize.y, 0, RenderTextureFormat.Default, 0)
                {
                    filterMode = FilterMode.Bilinear,
                    bindTextureMS = false,
                    antiAliasing = 1
                };
                buffer.CopyTexture(renderTarget, history);
            }
        }

        public void DisposeProperty()
        {
            Object.DestroyImmediate(HistoryRT);
            Object.DestroyImmediate(HistoryMotionVectorRT);
        }
    }
}
