using OpenTK;

namespace vsk.Rendering
{
    class Light
    {
        public Vector3 Position { get; }
        public Vector3 Color { get; }

        public Light(Vector3 position, Vector3 color)
        {
            Position = position;
            Color = color;
        }

    }
}
