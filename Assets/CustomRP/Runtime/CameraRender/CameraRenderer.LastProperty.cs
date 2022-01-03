using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{

    public sealed class CameraLastProperty
    {
        public float4x4 LastVP = Matrix4x4.identity;
        public float4x4 LastP;
        public float4x4 CameraLocalToWorld;
        
        public void SetupCameraLastProperty(Camera camera)
        {
            LastP = GraphicsUtility.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            CameraLocalToWorld = camera.transform.localToWorldMatrix;
            LastVP = mul(LastP, camera.worldToCameraMatrix);
        }
    }
}
