using CustomRP;
using CustomRP.Runtime;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{

    public sealed unsafe class CameraProperty
    {
        public float4x4 NonJitterP;
        public float4x4 P;
        public float4x4 WorldToView;
        public bool IsD3D;
        public float3 SceneOffset;

        public float4x4 VP;
        public float4x4 InverseVP;
        public float4x4 NonJitterVP;

        public float4x4 NonJitterInverseVP;
        public float4x4 NonJitterTextureVP;

        public float4x4 LastP;
        public float4x4 LastVP;
        public float4x4 LastInverseVP;
        public float4x4 LastCameraLocalToWorld;
        public float4x4 LastViewProjection { get; private set; }
        public float4x4 LastInverseViewProjection { get; private set; }
        
        public RenderTexture MotionVectorTextures;

        public readonly CameraLastProperty CameraLastProperty = new CameraLastProperty();
        public readonly HistoryTexture HistoryTexture = new HistoryTexture();
        public readonly PreviousDepthData PreviousDepthData = new PreviousDepthData();
        public readonly SSRCameraData SSRCameraData = new SSRCameraData();

        
        public void SetupCameraPreRender(Camera camera,float3 offset)
        {
            CameraLastProperty.SetupCameraLastProperty(camera);
            
            IsD3D = GraphicsUtility.platformIsD3D;
            NonJitterP = camera.nonJitteredProjectionMatrix;
            WorldToView = camera.worldToCameraMatrix;
            
            LastCameraLocalToWorld = CameraLastProperty.CameraLocalToWorld;
            LastP = CameraLastProperty.LastP;
            SceneOffset = offset;
            P = camera.projectionMatrix;
            
            float4x4 nonJitterPNoTex = GraphicsUtility.GetGPUProjectionMatrix(NonJitterP, false, IsD3D);
            NonJitterVP = mul(nonJitterPNoTex, WorldToView);
            NonJitterInverseVP = inverse(NonJitterVP);
            NonJitterTextureVP = mul(GraphicsUtility.GetGPUProjectionMatrix(NonJitterVP, true, IsD3D), WorldToView);
            LastCameraLocalToWorld.c3.xyz += offset;
            float4x4 lastV = GetWorldToCamera(ref LastCameraLocalToWorld);
            LastVP = mul(LastP, lastV);
            LastP = nonJitterPNoTex;
            LastInverseVP = inverse(LastVP);
            VP = mul(GraphicsUtility.GetGPUProjectionMatrix(P, false, IsD3D), WorldToView);
            InverseVP = inverse(VP);
        }


        public void SetupCameraRender(CommandBuffer buffer,Camera camera)
        {
            LastViewProjection = CameraLastProperty.LastVP;
            LastInverseViewProjection = LastInverseVP;
            buffer.SetGlobalMatrix(ShaderIds.LastVPId, LastViewProjection);
            buffer.SetGlobalMatrix(ShaderIds.NonJitterVPId, NonJitterVP);
            buffer.SetGlobalMatrix(ShaderIds.NonJitterTextureVPId, NonJitterTextureVP);
            buffer.SetGlobalMatrix(ShaderIds.InvNonJitterVPId, NonJitterInverseVP);
            buffer.SetGlobalMatrix(ShaderIds.InvVPId, InverseVP);
            CameraLastProperty.LastVP = NonJitterVP;
            CameraLastProperty.LastP = LastP;
            CameraLastProperty.CameraLocalToWorld = camera.transform.localToWorldMatrix;
        }
        
        private static float4x4 GetWorldToCamera(ref float4x4 localToWorldMatrix)
        {
            float4x4 worldToCameraMatrix = MathLib.GetWorldToLocal(ref localToWorldMatrix);
            float4 row2 = -float4(worldToCameraMatrix.c0.z, worldToCameraMatrix.c1.z, worldToCameraMatrix.c2.z, worldToCameraMatrix.c3.z);
            worldToCameraMatrix.c0.z = row2.x;
            worldToCameraMatrix.c1.z = row2.y;
            worldToCameraMatrix.c2.z = row2.z;
            worldToCameraMatrix.c3.z = row2.w;
            return worldToCameraMatrix;
        }
    }
}
