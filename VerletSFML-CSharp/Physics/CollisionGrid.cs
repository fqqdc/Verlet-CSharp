﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Verlet_CSharp.Engine.Common;

namespace Verlet_CSharp.Physics
{
    public class CollisionGrid
    {
        private CollisionCell[] cells;

        public int Width { get; init; } = 0;
        public int Height { get; init; } = 0;
        public int Size { get; init; } = 0;

        public ref CollisionCell this[int index] { get => ref cells[index]; }

        public CollisionGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Size = width * height;

            cells = new CollisionCell[width * height];
        }


        public bool AddAtom(int x, int y, int atom)
        {
            int id = x * Height + y; // 按列存储
            // Add to grid
            this[id].AddAtom(atom);
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < Height * Width; i++)
            {
                this[i].Clear();
            }
        }
    }
}
