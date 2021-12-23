using System;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustomRP.Examples
{
    public class MeshCube:MonoBehaviour
    {
        private static int s_BaseColorId = Shader.PropertyToID("_BaseColor");
        private static int s_MetallicId = Shader.PropertyToID("_Metallic");
        private static int s_SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static int s_CutoffId = Shader.PropertyToID("_Cutoff");
        
        [SerializeField]
        private Mesh m_Mesh = default;

        [SerializeField]
        private Material m_Material = default;

        private const int c_ConstCnt = 1023;
        
        private Matrix4x4[] m_Matrixs = new Matrix4x4[c_ConstCnt];

        private Vector4[] m_BaseColor = new Vector4[c_ConstCnt];

        private float[] m_Metallic = new float[c_ConstCnt];
        private float[] m_Smoothness = new float[c_ConstCnt];

        [SerializeField]
        private float m_Cutoff = 0.5f;
        
        private MaterialPropertyBlock m_Block;

        [SerializeField]
        private LightProbeProxyVolume m_LightProbeProxyVolume = null;
        private void Awake()
        {
            for (int i = 0; i < m_Matrixs.Length; i++)
            {
                m_Matrixs[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, 
                    Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f), 
                    Vector3.one * Random.Range(0.5f,1.5f));
                m_BaseColor[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
                m_Metallic[i] = Random.value < 0.25 ? 1F : 0f;
                m_Smoothness[i] = Random.Range(0.5f,0.95f);
            }
        }

        private void Update()
        {
            if (m_Block == null)
            {
                m_Block = new MaterialPropertyBlock();
                m_Block.SetVectorArray(s_BaseColorId,m_BaseColor);
                m_Block.SetFloatArray(s_MetallicId,m_Metallic);
                m_Block.SetFloatArray(s_SmoothnessId,m_Smoothness);
                if (!m_LightProbeProxyVolume)
                {
                    Vector3[] positions = new Vector3[c_ConstCnt];
                    for (var i = 0; i < m_Matrixs.Length; i++)
                    {
                        positions[i] = m_Matrixs[i].GetColumn(3);
                    }
                    SphericalHarmonicsL2[] lightProbes = new SphericalHarmonicsL2[c_ConstCnt];
                    Vector4[] occlusionProbes = new Vector4[c_ConstCnt];
                    LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions,lightProbes,occlusionProbes);
                    m_Block.CopySHCoefficientArraysFrom(lightProbes);
                    m_Block.CopyProbeOcclusionArrayFrom(occlusionProbes);
                }
                m_Block.SetFloat(s_CutoffId,m_Cutoff);
                
            }
            Graphics.DrawMeshInstanced(m_Mesh,0,m_Material,m_Matrixs,1023,m_Block,
                ShadowCastingMode.On,true,0,null,
                m_LightProbeProxyVolume ? LightProbeUsage.UseProxyVolume :LightProbeUsage.CustomProvided,m_LightProbeProxyVolume);
        }
    }
}