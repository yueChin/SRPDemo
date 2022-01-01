using CustomRP;
using CustomRP.Runtime;
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

    public class CameraProperty
    {
        public float4x4 NonJitterP;
        public float4x4 P;
        public float4x4 WorldToView;
        public float4x4 LastP;
        public float4x4 LastCameraLocalToWorld;
        public bool IsD3D;
        public float3 SceneOffset;

        public float4x4 VP;
        public float4x4 InverseVP;
        public float4x4 NonJitterVP;
        public float4x4 LastVP;

        public float4x4 NonJitterInverseVP;
        public float4x4 NonJitterTextureVP;
        public float4x4 LastInverseVP;

        public RenderTexture MotionVectorTextures;
        public RenderTexture HistoryTextures;
        public RenderTexture HistoryMotionVectorTextures;
        
        public void PreRender(Camera camera,float3 offset)
        {
            NonJitterP = camera.nonJitteredProjectionMatrix;
            P = camera.projectionMatrix;
            WorldToView = camera.worldToCameraMatrix;
            SceneOffset = offset;
            
            float4x4 nonJitterPNoTex = GraphicsUtility.GetGPUProjectionMatrix(NonJitterP, false, IsD3D);
            NonJitterVP = mul(nonJitterPNoTex, WorldToView);
            IsD3D = GraphicsUtility.platformIsD3D;
            NonJitterInverseVP = inverse(NonJitterVP);
            NonJitterTextureVP = mul(GraphicsUtility.GetGPUProjectionMatrix(NonJitterP, true, IsD3D), WorldToView);
            LastCameraLocalToWorld.c3.xyz += SceneOffset;
            float4x4 lastV = GetWorldToCamera(ref LastCameraLocalToWorld);
            LastVP = mul(LastP, lastV);
            LastP = nonJitterPNoTex;
            LastInverseVP = inverse(LastVP);
            VP = mul(GraphicsUtility.GetGPUProjectionMatrix(P, false, IsD3D), WorldToView);
            InverseVP = inverse(VP);
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
