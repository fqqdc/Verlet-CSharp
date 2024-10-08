﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Verlet_CSharp.Physics
{
    public class PhysicSolver
    {
        readonly List<PhysicObject> objects = [];
        readonly CollisionGrid grid;
        Vector2 worldSize = Vector2.Zero;
        Vector2 gravity = new(0, 20);
        readonly int sub_steps;

        readonly int threadCount = Environment.ProcessorCount;

        public int ObjectsCount { get => objects.Count; }
        public ref PhysicObject this[int index] { get => ref CollectionsMarshal.AsSpan(objects)[index]; }

        public int Width { get => (int)worldSize.X; }
        public int Height { get => (int)worldSize.Y; }

        public PhysicSolver(Vector2 size)
        {
            grid = new((int)size.X, (int)size.Y);
            worldSize = size;
            sub_steps = 8;

            grid.Clear();
        }

        // Checks if two atoms are colliding and if so create a new contact
        private void SolveContact(ref PhysicObject obj_1, ref PhysicObject obj_2)
        {
            const float response_coef = 1.0f;
            const float eps = 0.0001f;
            Vector2 o2_o1 = obj_1.Position - obj_2.Position;
            float dist2 = o2_o1.X * o2_o1.X + o2_o1.Y * o2_o1.Y;
            if (dist2 < 1.0f && dist2 > eps)
            {
                float dist = MathF.Sqrt(dist2);
                // Radius are all equal to 1.0f
                float delta = response_coef * 0.5f * (1.0f - dist);
                Vector2 col_vec = (o2_o1 / dist) * delta;
                obj_1.Position += col_vec;
                obj_2.Position -= col_vec;
            }
        }

        private void CheckAtomCellCollisions(int atom_idx, ref PhysicObject obj_1, ref CollisionCell c)
        {
            for (int i = 0; i < c.ObjectsCount; ++i)
            {
                int atom_2_idx = c.Objects[i];
                if (atom_idx == atom_2_idx)
                    continue;
                ref PhysicObject obj_2 = ref this[atom_2_idx];
                SolveContact(ref obj_1, ref obj_2);
            }
        }

        void ProcessCell(ref CollisionCell c, int index)
        {
            for (int i = 0; i < c.ObjectsCount; ++i)
            {
                int atom_idx = c.Objects[i];
                ref PhysicObject obj_1 = ref this[atom_idx];

                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index - 1]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index + 1]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index + grid.Height - 1]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index + grid.Height]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index + grid.Height + 1]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index - grid.Height - 1]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index - grid.Height]);
                CheckAtomCellCollisions(atom_idx, ref obj_1, ref grid[index - grid.Height + 1]);
            }
        }

        private void SolveCollisionThreaded(int i, int sliceSize)
        {
            int start = i * sliceSize;

            int end = (i + 1) * sliceSize;
            end = Math.Min(end, grid.Size);

            for (int idx = start; idx < end; ++idx)
            {
                ProcessCell(ref grid[idx], idx);
            }
        }

        // Find colliding atoms
        private void SolveCollisionsParallel()
        {
            int sliceCount = threadCount * 2;
            int sliceSize = (grid.Width / sliceCount + 1) * grid.Height;

            // Find collisions in two passes to avoid data races
            // First collision pass
            Parallel.For(0, threadCount, i =>
            {
                SolveCollisionThreaded(2 * i, sliceSize);
            });
            // Second collision pass
            Parallel.For(0, threadCount, i =>
            {
                SolveCollisionThreaded(2 * i + 1, sliceSize);
            });
        }

        private void SolveCollisions()
        {
            for (int i = 0; i < grid.Width; i++)
            {
                SolveCollisionThreaded(i, grid.Height);
            }
        }

        // Add a new object to the solver
        public int AddObject(PhysicObject obj)
        {
            objects.Add(obj);
            return objects.Count - 1;
        }

        // Add a new object to the solver
        public int CreateObject(Vector2 pos)
        {
            return AddObject(new(pos));
        }

        public void Update(float dt)
        {
            // Perform the sub steps
            float sub_dt = dt / sub_steps;
            for (int i = sub_steps; i > 0; i--)
            {
                AddObjectsToGrid();
                SolveCollisionsParallel();
                UpdateObjectsParallel(sub_dt);
            }
        }

        public void UpdateNotParallel(float dt)
        {
            // Perform the sub steps
            float sub_dt = dt / sub_steps;
            for (int i = sub_steps; i > 0; i--)
            {
                AddObjectsToGrid();
                SolveCollisions();
                UpdateObjects(sub_dt);
            }
        }

        private void AddObjectsToGrid()
        {
            grid.Clear();
            // Safety border to avoid adding object outside the grid
            int i = 0;
            var arrObject = CollectionsMarshal.AsSpan(objects);
            for (int j = 0; j < arrObject.Length; j++)
            {
                ref PhysicObject obj = ref arrObject[j];
                if (obj.Position.X > 1.0f && obj.Position.X < worldSize.X - 1.0f &&
                    obj.Position.Y > 1.0f && obj.Position.Y < worldSize.Y - 1.0f)
                {
                    grid.AddAtom((int)obj.Position.X, (int)obj.Position.Y, i);
                }
                ++i;
            }
        }

        private void UpdateObjectsParallel(float dt)
        {
            if (objects.Count == 0) return;

            var partitionerSize = (objects.Count + threadCount - 1) / threadCount;
            var partitioner = Partitioner.Create(0, objects.Count, partitionerSize);

            Parallel.ForEach(partitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; ++i)
                {
                    ref PhysicObject obj = ref this[i];
                    // Add gravity
                    obj.Acceleration += gravity;
                    // Apply Verlet integration
                    obj.Update(dt);
                    // Apply map borders collisions
                    const float margin = 2.0f;

                    var vel = obj.Velocity;
                    if (obj.Position.X > worldSize.X - margin)
                    {
                        obj.Position.X = worldSize.X - margin;

                        vel.X *= -1;
                        obj.SetVelocity(vel);
                    }
                    else if (obj.Position.X < margin)
                    {
                        obj.Position.X = margin;

                        vel.X *= -1;
                        obj.SetVelocity(vel);
                    }
                    if (obj.Position.Y > worldSize.Y - margin)
                    {
                        obj.Position.Y = worldSize.Y - margin;

                        vel.Y *= -1;
                        obj.SetVelocity(vel);
                    }
                    else if (obj.Position.Y < margin)
                    {
                        obj.Position.Y = margin;

                        vel.Y *= -1;
                        obj.SetVelocity(vel);
                    }
                }
            });
        }

        private void UpdateObjects(float dt)
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                ref PhysicObject obj = ref this[i];
                // Add gravity
                obj.Acceleration += gravity;
                // Apply Verlet integration
                obj.Update(dt);
                // Apply map borders collisions
                const float margin = 2.0f;
                if (obj.Position.X > worldSize.X - margin)
                {
                    obj.Position.X = worldSize.X - margin;
                }
                else if (obj.Position.X < margin)
                {
                    obj.Position.X = margin;
                }
                if (obj.Position.Y > worldSize.Y - margin)
                {
                    obj.Position.Y = worldSize.Y - margin;
                }
                else if (obj.Position.Y < margin)
                {
                    obj.Position.Y = margin;
                }
            }
        }
    }
}
