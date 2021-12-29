using UnityEngine;

public static partial class ShaderIds
{
    public static readonly int ColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    public static readonly int ColorTextureId = Shader.PropertyToID("_CameraTexture");
    public static readonly int DepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    public static readonly int DepthTextureId = Shader.PropertyToID("_CameraDepthTexture");

    public static readonly int SourceTextureId = Shader.PropertyToID("_SourceTexture");
    public static readonly int MotionVectorsTextureId = Shader.PropertyToID("_CameraMotionVectorsTexture");
    
    public static readonly int InvVPId = Shader.PropertyToID("_InvVP");
    public static readonly int InvNonJitterVPId = Shader.PropertyToID("_InvNonJitterVP");
    public static readonly int LastVPId = Shader.PropertyToID("_LastVp");
    public static readonly int InvLastVPId = Shader.PropertyToID("_InvLastVp");
    
    public static readonly int LastFrameDepthTextureId = Shader.PropertyToID("_LastFrameDepthTexture");
    public static readonly int LastFrameMotionVectorsId = Shader.PropertyToID("_LastFrameMotionVectors");
    
    public static readonly int TemporalClipBoundingId = Shader.PropertyToID("_TemporalClipBounding");
}