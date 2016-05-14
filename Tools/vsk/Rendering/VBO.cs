using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Render
{
    internal class VBO
    {
        public BufferTarget target;
        public int ElementSize { get; private set; }
        public int Handle { get; private set; }

        public void Buffer<T>(BufferTarget target, T[] buffer, int elementSize, BufferUsageHint hint) where T : struct
        {
            ElementSize = elementSize;
            this.target = target;
            Handle = GL.GenBuffer();
            GL.BindBuffer(target, Handle);
            GL.BufferData(target,
                new IntPtr(buffer.Length*Marshal.SizeOf(typeof (T))),
                buffer, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(target, Handle);
        }
    }
}