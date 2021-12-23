namespace CustomRP
{
    public partial class ShadowSettings
    {
        public enum TextureSize 
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8196 = 8196
        }
        
        public enum FilterMode
        {
            PCF2x2,
            PCF3x3,
            PCF5x5,
            PCF7x7,
        }

        public enum CascadeBlendMode
        {
            Hard,
            Soft,
            Dither,
        }
    }
}