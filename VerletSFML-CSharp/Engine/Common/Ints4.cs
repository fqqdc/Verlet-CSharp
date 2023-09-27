using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VerletSFML_CSharp.Engine.Common
{
    public struct Ints4
    {
        public int int_0;
        public int int_1;
        public int int_2;
        public int int_3;
    }

    public static class Ints4Helper
    {
        public static ref int Item(this ref Ints4 ints4, int index)
        {
            var spanInts4 = new Span<Ints4>(ref ints4);
            return ref MemoryMarshal.Cast<Ints4, int>(spanInts4)[index];
        }
    }
}
