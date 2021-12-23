using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor m_Editor;
    private object[] m_Materials;
    private MaterialProperty[] m_Propertys;
    private bool m_IsShowPresets;

    private void SetShadowCasterPass()
    {
        MaterialProperty shadows = FindProperty("_Shadows", m_Propertys, false);
        if (shadows == null || shadows.hasMixedValue)
        {
            return;
        }

        bool enabled = shadows.floatValue < (float)ShadowMode.Off;
        foreach (Material material in m_Materials)
        {
            material.SetShaderPassEnabled("ShadowCaster", enabled);
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUI.BeginChangeCheck();
        base.OnGUI(materialEditor, properties);
        m_Editor = materialEditor;
        m_Materials = materialEditor.targets;
        m_Propertys = properties;

        BakedEmission();
        EditorGUILayout.Space();
        m_IsShowPresets = EditorGUILayout.Foldout(m_IsShowPresets, "Presets", true);
        if (m_IsShowPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }

        if (EditorGUI.EndChangeCheck())
        {
            SetShadowCasterPass();
            CopyLightMappingProperties();
        }
    }

    private enum ShadowMode
    {
        On,
        Clip,
        Dither,
        Off,
    }

    ShadowMode Shadows
    {
        set
        {
            if (SetProperty("_Shadows", (float)value))
            {
                SetKeyWord("_SHADOWS_CLIP", value == ShadowMode.Clip);
                SetKeyWord("_SHADOWS_DITHER", value == ShadowMode.Dither);
            }
        }
    }

    private bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, m_Propertys, false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }

        return false;
    }

    private void SetKeyWord(string keyWord, bool enable)
    {
        if (enable)
        {
            foreach (Material material in m_Materials)
            {
                material.EnableKeyword(keyWord);
            }
        }
        else
        {
            foreach (Material material in m_Materials)
            {
                material.DisableKeyword(keyWord);
            }
        }
    }

    private void SetProperty(string name, string keyWord, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyWord(name, value);
        }
    }

    private bool PressButton(string name)
    {
        if (GUILayout.Button(name))
        {
            m_Editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    private void OpaquePreset()
    {
        if (PressButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    private void ClipPreset()
    {
        if (PressButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    private void FadePreset()
    {
        if (PressButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void TransparentPreset()
    {
        if (HasPremultiplyAlpha && PressButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void BakedEmission()
    {
        EditorGUI.BeginChangeCheck();
        m_Editor.LightmapEmissionProperty();
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Material target in m_Editor.targets)
            {
                target.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    }

    private void CopyLightMappingProperties()
    {
        MaterialProperty mainTex = FindProperty("_MainTex", m_Propertys, false);
        MaterialProperty baseMap = FindProperty("_BaseMap", m_Propertys, false);
        if (mainTex != null && baseMap != null)
        {
            mainTex.textureValue = baseMap.textureValue;
            mainTex.textureScaleAndOffset = baseMap.textureScaleAndOffset;
        }
        MaterialProperty color = FindProperty("_Color", m_Propertys, false);
        MaterialProperty baseColor = FindProperty("_BaseColor", m_Propertys, false);
        if (color != null && baseColor != null)
        {
            color.colorValue = baseColor.colorValue;
        }
    }

    private bool HasProperty(string name) => FindProperty(name, m_Propertys, false) != null;
    private bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");
    private bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }
    
    private bool PremultiplyAlpha
    {
        set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value);
    }
    
    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend",(float) value);
    }
    
    private BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float) value);
    }
    
    private bool ZWrite
    {
        set => SetProperty("_ZWrite", "_ZWRITE", value);
    }
    
    private RenderQueue RenderQueue
    {
        set
        {
            foreach (Material material in m_Materials)
            {
                material.renderQueue = (int)value;
            }
        }
    }
    
}
