using CustomRP;
using CustomRP.CameraRender;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{
    public sealed class SSRCameraData :IDisposeProperty
    {
        public Vector2 BufferSize { get; private set; }
        public ColorLUTResolution RayCastingResolution { get; private set; }
        public RenderTexture SSRTemporalPrevRT, SSRHierarchicalDepthRT, SSRHierarchicalDepthBackUpRT;
        
        public void SetupSSRCameraData(Vector2Int currentSize, ColorLUTResolution targetResolution)
        {
            BufferSize = currentSize;
            RayCastingResolution = targetResolution;

            SSRHierarchicalDepthRT = new RenderTexture(currentSize.x, currentSize.y, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                useMipMap = true,
                autoGenerateMips = false
            };
            SSRHierarchicalDepthRT.Create();

            SSRHierarchicalDepthBackUpRT = new RenderTexture(currentSize.x, currentSize.y, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                useMipMap = true,
                autoGenerateMips = false
            };
            SSRHierarchicalDepthBackUpRT.Create();

            SSRTemporalPrevRT = new RenderTexture(currentSize.x, currentSize.y, 0, RenderTextureFormat.ARGBHalf)
            {
                filterMode = FilterMode.Bilinear
            };
            SSRTemporalPrevRT.Create();
        }
        
        public bool SetupSize(Vector2Int currentSize, ColorLUTResolution targetResolution)
        {
            if (BufferSize == currentSize && RayCastingResolution == targetResolution)
            {
                return false;
            }
            BufferSize = currentSize;
            RayCastingResolution = targetResolution;
            ChangeSet(SSRHierarchicalDepthRT, currentSize.x, currentSize.y, 0, RenderTextureFormat.RFloat);
            ChangeSet(SSRHierarchicalDepthBackUpRT, currentSize.x, currentSize.y, 0, RenderTextureFormat.RFloat);
            ChangeSet(SSRTemporalPrevRT, currentSize.x, currentSize.y, 0, RenderTextureFormat.ARGBHalf);
            return true;
        }
        
        public static void ChangeSet(RenderTexture targetRT, int width, int height, int depth, RenderTextureFormat format)
        {
            targetRT.Release();
            targetRT.width = width;
            targetRT.height = height;
            targetRT.depth = depth;
            targetRT.format = format;
            targetRT.Create();
        }
        
        public static void CheckAndRelease(ref RenderTexture targetRT)
        {
            if (targetRT && targetRT.IsCreated())
            {
                Object.DestroyImmediate(targetRT);
                targetRT = null;
            }
        }
        
        public void DisposeProperty()
        {
            CheckAndRelease(ref SSRTemporalPrevRT);
            CheckAndRelease(ref SSRHierarchicalDepthRT);
            CheckAndRelease(ref SSRHierarchicalDepthBackUpRT);
        }
    }
}
