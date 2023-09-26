using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerletSFML_CSharp.Physics
{
    public class CollisionCell
    {
        const int CellCapacity = 4;

        private int[] objects;

        public CollisionCell() {
            objects = new int[CellCapacity];
        }

        public int ObjectsCount { get; private set; }

        public int this[int index]
        {
            get => objects[index];
        }

        public void AddAtom(int id)
        {
            objects[ObjectsCount] = id;
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
                if (objects[i] == id)
                {
                    // Swap pop
                    objects[i] = objects[ObjectsCount - 1];
                    --ObjectsCount;
                    return;
                }
            }
        }
    }
}
