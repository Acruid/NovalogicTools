using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    internal class VBO
    {
        private BufferTarget Target { get; set; }
        public int ElementSize { get; private set; }
        private int Handle { get; set; }

        public void Buffer<T>(BufferTarget target, T[] buffer, int elementSize, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct
        {
            ElementSize = elementSize;
            Target = target;

            Handle = GL.GenBuffer();
            GL.BindBuffer(target, Handle);
            GL.BufferData(target,
                new IntPtr(buffer.Length*Marshal.SizeOf(typeof(T))),
                buffer, hint);
        }

        public void Bind()
        {
            GL.BindBuffer(Target, Handle);
        }
    }
}