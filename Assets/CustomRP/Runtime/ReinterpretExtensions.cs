using System.Runtime.InteropServices;

namespace CustomRP
{
    public static class ReinterpretExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct IntFloat
        {
            [FieldOffset(0)]
            public int IntValue;
            [FieldOffset(0)]
            public float FloatValue;
        }
        
        public static float ReinterpertAsFloat(this int value)
        {
            IntFloat converter = default;
            converter.IntValue = value;
            return converter.FloatValue;
        }
    }
}