using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VerletSFML_CSharp.Physics
{
    public struct PhysicObject
    {
        public Vector2 Position = new(0f, 0f);
        public Vector2 LastPosition = new(0f, 0f);
        public Vector2 Acceleration = new(0f, 0f);
        public Color Color;

        public void SetPosition(Vector2 pos)
        {
            Position = pos;
            LastPosition = pos;
        }

        public PhysicObject(Vector2 position)
        {
            this.Position = position;
            this.LastPosition = position;
        }

        public void Update(float dt)
        {
            Vector2 lastUpdateMove = Position - LastPosition;
            Vector2 newPosition = Position + lastUpdateMove + (Acceleration - lastUpdateMove * 40.0f) * (dt * dt);
            LastPosition = Position;
            Position = newPosition;
            Acceleration = new(0f, 0f);
        }

        public void Stop()
        {
            LastPosition = Position;
        }

        public void Slowdown(float ratio)
        {
            LastPosition = LastPosition + ratio * (Position - LastPosition);
        }

        public float Speed { get => Vector2.Distance(Position, LastPosition); }
        public Vector2 Velocity { get => Position - LastPosition; }

        public void AddVelocity(Vector2 v)
        {
            LastPosition -= v;
        }

        public void SetVelocity(Vector2 v)
        {
            LastPosition = Position - v;
        }

        public void SetPositionSameSpeed(Vector2 newPosition)
        {
            Vector2 toLast = LastPosition - Position;
            Position = newPosition;
            LastPosition = Position + toLast;
        }

        public void Move(Vector2 v)
        {
            Position += v;
        }
    }
}
