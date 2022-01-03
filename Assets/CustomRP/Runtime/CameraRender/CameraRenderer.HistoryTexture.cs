using CustomRP;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{
    public sealed class HistoryTexture
    {
        public RenderTexture HistoryRT;
        public RenderTexture HistoryMotionVectorRT;
        public Vector2 Jitter;    
        
        public void SetupHistoryTexture(Vector2Int buffSize,bool useHDR)
        {
            HistoryRT = new RenderTexture(buffSize.x, buffSize.y, 0,useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default, 0)
            {
                bindTextureMS = false
            };
            HistoryRT.Create();

            HistoryMotionVectorRT = new RenderTexture(buffSize.x, buffSize.y, 0, RenderTextureFormat.Depth, 0)
            {
                bindTextureMS = false
            };
            HistoryMotionVectorRT.Create();
        }
        
        public void SetupProperty(Camera camera,Vector2Int buffSize)
        {
            int bufferWidth = buffSize.x;
            int camHeight = buffSize.y;
            if (!HistoryRT)
            {
                HistoryRT = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.Default, 0)
                {
                    bindTextureMS = false
                };
                HistoryRT.Create();
            }

            if (!HistoryMotionVectorRT)
            {
                HistoryMotionVectorRT = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.Depth, 0)
                {
                    bindTextureMS = false
                };
                HistoryMotionVectorRT.Create();
            }
            Resize(HistoryRT, bufferWidth, camHeight);
            Resize(HistoryMotionVectorRT, bufferWidth, camHeight);
        }
        
        private static void Resize(RenderTexture rt, int width, int height)
        {
            if (rt.width == width && rt.height == height)
            {
                return;
            }
            rt.Release();
            rt.width = width;
            rt.height = height;
            rt.Create();
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
    }
}
