using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
