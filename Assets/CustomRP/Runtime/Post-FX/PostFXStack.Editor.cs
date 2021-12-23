using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class PostFXStack
    {
#if UNITY_EDITOR
        partial void ApplySceneViewState()
        {
            if (m_Camera.cameraType == CameraType.SceneView &&
                !SceneView.currentDrawingSceneView.sceneViewState.showImageEffects)
            {
                m_PostFXSettings = null;
            }
        }
#endif
    }
}