using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int s_BaseColorId = Shader.PropertyToID("_BaseColor");
    private static int s_CutoffId = Shader.PropertyToID("_Cutoff");
    private static int s_Metallic = Shader.PropertyToID("_Metallic");
    private static int s_Smoothness = Shader.PropertyToID("_Smoothness");
    
    [SerializeField]
    private Color m_BaseColor = Color.white;

    [SerializeField,Range(0f,1f)]
    private float m_Cutoff = 0.5f;

    [SerializeField,Range(0,1f)]
    private float m_Metallic = 0f;

    [SerializeField,Range(0,1f)]
    private float m_Smoothness = 0.5f;
    
    private static MaterialPropertyBlock m_Block;

    private static int s_EmissionColorId = Shader.PropertyToID("_Emission");
    
    [SerializeField,ColorUsage(false,true)]
    private Color m_EmissionColor = Color.black;
    

    private void OnValidate()
    {
        if (m_Block == null)
        {
            m_Block = new MaterialPropertyBlock();
        }
        
        m_Block.SetColor(s_BaseColorId,m_BaseColor);
        m_Block.SetFloat(s_CutoffId,m_Cutoff);
        m_Block.SetFloat(s_Metallic,m_Metallic);
        m_Block.SetFloat(s_Smoothness,m_Smoothness);
        m_Block.SetColor(s_EmissionColorId,m_EmissionColor);
        GetComponent<Renderer>().SetPropertyBlock(m_Block);
    }

    private void Awake()
    {
        OnValidate();
    }
}
