using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verlet_CSharp.Engine.Common;

namespace Verlet_CSharp.Physics
{
    public struct CollisionCell
    {
        const int CellCapacity = 4;

        public Ints4 Objects;

        public int ObjectsCount { get; private set; }

        public void AddAtom(int id)
        {
            Objects[ObjectsCount] = id;
            if (ObjectsCount < CellCapacity - 1)
                ObjectsCount += 1;
        }

        public void Clear()
        {
            ObjectsCount = 0;
        }

        public void Remove(int id)
        {
            for (int i = 0; i < ObjectsCount; ++i)
            {
                if (Objects[i] == id)
                {
                    // Swap pop
                    Objects[i] = Objects[ObjectsCount - 1];
                    --ObjectsCount;
                    return;
                }
            }
        }
    }
}
