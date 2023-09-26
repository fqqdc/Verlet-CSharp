using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VerletSFML_CSharp.Engine.Common
{
    public static class ColorUtils
    {
        public static Color CreateColor(Vector3 vec)
        {
            return Color.FromRgb((byte)vec.X, (byte)vec.Y, (byte)vec.Z);
        }

        public static Color GetRainbow(float t)
        {
            float r = MathF.Sin(t);
            float g = MathF.Sin(t + 0.33f * 2.0f * MathF.PI);
            float b = MathF.Sin(t + 0.66f * 2.0f * MathF.PI);
            return CreateColor(new(255 * r * r, 255 * g * g, 255 * b * b));
        }
    }
}
