﻿using System.Runtime.CompilerServices;

namespace Verlet_CSharp.Physics
{

    [InlineArray(CollisionCell.CellCapacity)]
    public struct SubCells
    {
#pragma warning disable IDE0051 // 删除未使用的私有成员
#pragma warning disable IDE0044 // 添加只读修饰符
        int _elememt;
#pragma warning restore IDE0044 // 添加只读修饰符
#pragma warning restore IDE0051 // 删除未使用的私有成员
    }


    public struct CollisionCell
    {
        internal const int CellCapacity = 4;

        public SubCells Objects;

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
