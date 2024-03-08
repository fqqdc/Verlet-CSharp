using System;
using System.Numerics;

namespace Verlet_CSharp.Engine.Common
{
    public static class ColorUtils
    {
        public static Pixel24 CreateColor(Vector3 vec)
        {
            return new() { R = (byte)vec.X, G = (byte)vec.Y, B = (byte)vec.Z };
        }

        public static Pixel24 GetRainbow(float t)
        {
            float r = MathF.Sin(t);
            float g = MathF.Sin(t + 0.33f * 2.0f * MathF.PI);
            float b = MathF.Sin(t + 0.66f * 2.0f * MathF.PI);
            return CreateColor(new(255 * r * r, 255 * g * g, 255 * b * b));
        }
    }
}
