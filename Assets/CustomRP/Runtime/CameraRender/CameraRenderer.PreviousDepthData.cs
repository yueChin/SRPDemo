using UnityEngine;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{
    public sealed class PreviousDepthData
    {
        public Vector2 BufferSize { get; private set; }
        public RenderTexture SSRPrevDepthRT;
        
        public void SetupPreviousDepthData(Vector2Int currentSize)
        {
            BufferSize = currentSize;
            SSRPrevDepthRT = new RenderTexture(currentSize.x, currentSize.y, 0, RenderTextureFormat.Depth)
            {
                filterMode = FilterMode.Bilinear
            };
            SSRPrevDepthRT.Create();
        }
        
        public bool SetupData(Vector2Int currentSize)
        {
            if (BufferSize == currentSize)
            {
                return false;
            }
            BufferSize = currentSize;
            SSRCameraData.ChangeSet(SSRPrevDepthRT, currentSize.x, currentSize.y, 0, RenderTextureFormat.Depth);
            return true;
        }
        
        public void DisposeProperty()
        {
            SSRCameraData.CheckAndRelease(ref SSRPrevDepthRT);
        }
    }
}
