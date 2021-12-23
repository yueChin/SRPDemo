using UnityEngine;

namespace CustomRP
{
    [CreateAssetMenu(menuName = "Rendering/PostFXSettings",fileName = "CustomPostFXSettings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField]
        private Shader m_Shader = default;

        [System.NonSerialized]
        private Material m_Material;

        [SerializeField] 
        private ToneMappingSettings m_ToneMapping = default;

        [SerializeField]
        private ColorAdjustmentsSettings m_ColorAdjustmentSettings = new ColorAdjustmentsSettings()
        {
            ColorFilter = Color.white,
        };

        [SerializeField]
        private WhiteBalanceSettings m_WitheBalanceSettings = default;

        [SerializeField]
        private SplitToningSettings m_SplitToningSettings = new SplitToningSettings()
        {
            Shadows = Color.gray,
            HightLights = Color.gray,
        };  
        
        [SerializeField]
        private ChannelMixerSettings m_ChannelMixerSettings = new ChannelMixerSettings()
        {
            Red = Vector3.left,
            Green = Vector3.up,
            Blue = Vector3.left,
        };

        [SerializeField]
        private ShadowsMidtonesHighlightsSettings m_ShadowsMidtonesHighlightsSettings =
            new ShadowsMidtonesHighlightsSettings()
            {
                Shadows = Color.white,
                Midtones = Color.white,
                Highlights = Color.white,
                ShadowsEnd = 0.3f,
                HighlightsStart = 0.55f,
                HighlightEnd = 1f,
            };
        
        [SerializeField] 
        private BloomSettings m_Bloom = new BloomSettings()
        {
            Scatter = 0.7f,
        };
        
        public Material Material
        {
            get
            {
                if (m_Material != null)
                {
                    return m_Material;
                }
                
                if (m_Shader != null)
                {
                    m_Material = new Material(m_Shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }

                return m_Material;
            }
        }

        public BloomSettings Bloom => m_Bloom;

        public ToneMappingSettings ToneMapping => m_ToneMapping;

        public ColorAdjustmentsSettings ColorAdjustmentsSettings => m_ColorAdjustmentSettings;

        public WhiteBalanceSettings WhiteBalanceSettings => m_WitheBalanceSettings;

        public SplitToningSettings SplitToningSettings => m_SplitToningSettings;

        public ChannelMixerSettings ChannelMixerSettings => m_ChannelMixerSettings;

        public ShadowsMidtonesHighlightsSettings ShadowsMidtonesHighlightsSettings => m_ShadowsMidtonesHighlightsSettings;
    }

  
}