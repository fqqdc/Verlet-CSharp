namespace Verlet_CSharp.Physics
{
    public class CollisionGrid(int width, int height)
    {
        private readonly CollisionCell[] cells = new CollisionCell[width * height];

        public int Width { get; init; } = width;
        public int Height { get; init; } = height;
        public int Size { get; init; } = width * height;

        public ref CollisionCell this[int index] { get => ref cells[index]; }

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
