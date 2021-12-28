using UnityEngine;

public static partial class ShaderIds
{
    public static readonly int ColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    public static readonly int ColorTextureId = Shader.PropertyToID("_CameraTexture");
    public static readonly int DepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    public static readonly int DepthTextureId = Shader.PropertyToID("_CameraDepthTexture");
    public static readonly int SourceTextureId = Shader.PropertyToID("_SourceTexture");
    public static readonly int MotionVectorsTextureId = Shader.PropertyToID("_CameraMotionVectorsTexture");
}