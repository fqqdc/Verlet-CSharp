using System.Numerics;
using Verlet_CSharp.Engine.Common;

namespace Verlet_CSharp.Physics
{
    public struct PhysicObject(Vector2 position)
    {
        public Vector2 Position = position;
        public Vector2 LastPosition = position;
        public Vector2 Acceleration = new(0f, 0f);
        public Pixel24 Color;

        public void SetPosition(Vector2 pos)
        {
            Position = pos;
            LastPosition = pos;
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
            LastPosition += ratio * (Position - LastPosition);
        }

        public readonly float Speed { get => Vector2.Distance(Position, LastPosition); }
        public readonly Vector2 Velocity { get => Position - LastPosition; }

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
