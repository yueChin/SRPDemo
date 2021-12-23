using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CanEditMultipleObjects]
[CustomEditorForRenderPipeline(typeof(Light),typeof(CustomRenderPipelineAsset))]
public class CustomLightEditor : LightEditor
{
    private static GUIContent s_RenderingLayerMaskLabel = new GUIContent("Rendering Layer Mask","Functional version of above property.");
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //DrawRenderingLayerMask();
        RenderingLayerMaskDrawer.Draw(settings.renderingLayerMask,s_RenderingLayerMaskLabel);
        if (!settings.lightType.hasMultipleDifferentValues && (LightType)settings.lightType.enumValueIndex == LightType.Spot)
        {
            settings.DrawInnerAndOuterSpotAngle();
            settings.ApplyModifiedProperties();
        }
        settings.ApplyModifiedProperties();
        Light light = target as Light;
        if (light.cullingMask != -1)
        {
            EditorGUILayout.HelpBox(light.type == LightType.Directional 
                ? "Culling Mask Only Affects Shadows" 
                : "Culling Mask Only Affects Shadows Unless Use Lights Per Objects is On",MessageType.Warning);
        }
    }

    private void DrawRenderingLayerMask()
    {
        SerializedProperty property = settings.renderingLayerMask;
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();
        int mask = property.intValue;
        if (mask == int.MaxValue)
        {
            mask = -1;
        }
        mask = EditorGUILayout.MaskField(s_RenderingLayerMaskLabel,mask,GraphicsSettings.currentRenderPipeline.renderingLayerMaskNames);
        if (EditorGUI.EndChangeCheck())
        {
            property.intValue = mask == -1 ? int.MaxValue : mask;
        }

        EditorGUI.showMixedValue = false;
    }
}