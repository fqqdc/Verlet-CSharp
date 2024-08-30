using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Verlet_CSharp.Engine.Common
{
    public struct Pixel24
    {
        public byte B;
        public byte G;
        public byte R;
    }

    public static class DrawHelper
    {
        public static void FillCircle(this Span<Pixel24> data, int dataWidth, float x_c, float y_c, float r, Pixel24 color)
        {
            int dataHeight = data.Length / dataWidth;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int XY2Index(int x, int y) => dataWidth * y + x;

            //Bresenham中点画圆算法
            var (x0, y0) = (0, r);
            var p = 1 - r;
            var y = y0;
            for (var x = x0; x < y; x++)
            {
                if (p < 0)
                {
                    // 中点在园内
                    p += 2 * x + 1;
                }
                else
                {
                    // 中点在圆外或圆上
                    y -= 1;
                    p += 2 * (x - y) + 1;
                }
                //setPixel(x_c + x, y_c + y);
                //setPixel(x_c - x, y_c + y);
                //setPixel(x_c - x, y_c - y);
                //setPixel(x_c + x, y_c - y);
                for (int p_x = (int)(x_c - x); p_x <= x_c + x; p_x++)
                {
                    if (x < 0 || x >= dataWidth || y < 0 || y >= dataHeight)
                        continue;

                    //setPixel(p_x, (int)(y_c + y));
                    //setPixel(p_x, (int)(y_c - y));
                    int index = XY2Index(p_x, (int)(y_c + y));
                    Debug.Assert(index >= 0 && index < data.Length);
                    data[index] = color;
                    index = XY2Index(p_x, (int)(y_c - y));
                    Debug.Assert(index >= 0 && index < data.Length);
                    data[XY2Index(p_x, (int)(y_c - y))] = color;
                }

                //setPixel(x_c + y, y_c + x);
                //setPixel(x_c - y, y_c + x);
                //setPixel(x_c + y, y_c - x);
                //setPixel(x_c - y, y_c - x);
                for (int p_x = (int)(x_c - y); p_x <= (int)(x_c + y); p_x++)
                {
                    if (x < 0 || x >= dataWidth || y < 0 || y >= dataHeight)
                        continue;
                    //setPixel(p_x, (int)(y_c + x));
                    //setPixel(p_x, (int)(y_c - x));
                    int index = XY2Index(p_x, (int)(y_c + x));
                    Debug.Assert(index >= 0 && index < data.Length);
                    data[index] = color;
                    index = XY2Index(p_x, (int)(y_c - x));
                    Debug.Assert(index >= 0 && index < data.Length);
                    data[index] = color;
                }
            }
        }

        public static void DrawCircle(this Pixel24[] data, int dataWidth, float x_c, float y_c, float r, Pixel24 color)
        {
            int dataHeight = data.Length / dataWidth;

            void setPixel(int x, int y)
            {
                if (x < 0 || x >= dataWidth || y < 0 || y >= dataHeight)
                    return;

                data[dataWidth * y + x] = color;
            }

            //Bresenham中点画圆算法
            var (x0, y0) = (0, r);
            var p = 1 - r;
            var y = y0;
            for (var x = x0; x < y; x++)
            {
                if (p < 0)
                {
                    // 中点在园内
                    p += 2 * x + 1;
                }
                else
                {
                    // 中点在圆外或圆上
                    y -= 1;
                    p += 2 * (x - y) + 1;
                }
                setPixel((int)(x_c + x), (int)(y_c + y));
                setPixel((int)(x_c - x), (int)(y_c + y));
                setPixel((int)(x_c - x), (int)(y_c - y));
                setPixel((int)(x_c + x), (int)(y_c - y));
                setPixel((int)(x_c + y), (int)(y_c + x));
                setPixel((int)(x_c - y), (int)(y_c + x));
                setPixel((int)(x_c + y), (int)(y_c - x));
                setPixel((int)(x_c - y), (int)(y_c - x));
            }
        }
    }
}
