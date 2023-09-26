using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VerletSFML_CSharp.Physics
{
    public class PhysicObject
    {
        private Vector2 position = new(0f, 0f);
        private Vector2 lastPosition = new(0f, 0f);
        private Vector2 acceleration = new(0f, 0f);
        private Color color;

        public void SetPosition(Vector2 pos)
        {
            position = pos;
            lastPosition = pos;
        }

        public PhysicObject(Vector2 position)
        {
            this.position = position;
            this.lastPosition = position;
        }

        public ref Vector2 Position { get => ref this.position; }
        public ref Vector2 Acceleration { get => ref this.acceleration; }
        public ref Color Color { get => ref this.color; }

        public void Update(float dt)
        {
            Vector2 lastUpdateMove = position - lastPosition;
            Vector2 newPosition = position + lastUpdateMove + (acceleration - lastUpdateMove * 40.0f) * (dt * dt);
            lastPosition = position;
            position = newPosition;
            acceleration = new(0f, 0f);
        }

        public void Stop()
        {
            lastPosition = position;
        }

        public void Slowdown(float ratio)
        {
            lastPosition = lastPosition + ratio * (position - lastPosition);
        }

        public float Speed { get => Vector2.Distance(position, lastPosition); }
        public Vector2 Velocity { get => position - lastPosition; }

        public void AddVelocity(Vector2 v)
        {
            lastPosition -= v;
        }

        public void SetPositionSameSpeed(Vector2 newPosition)
        {
            Vector2 toLast = lastPosition - position;
            position = newPosition;
            lastPosition = position + toLast;
        }

        public void Move(Vector2 v)
        {
            position += v;
        }
    }
}
