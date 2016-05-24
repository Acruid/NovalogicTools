using System;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    internal class VAO
    {
        private bool _indexed;

        public VAO(PrimitiveType drawType, int numVerts)
        {
            Handle = GL.GenVertexArray();
            DrawType = drawType;
            NumVerts = numVerts;
        }

        private int Handle { get; }
        public bool DisableDepth { get; set; }

        private int NumVerts { get; }
        private PrimitiveType DrawType { get; }

        public void Use()
        {
            if (Handle != 0)
                GL.BindVertexArray(Handle);
            else
                throw new Exception("VBO handle is null.");
        }

        public void EnableAttrib(int index)
        {
            GL.EnableVertexAttribArray(0);
        }

        public void AddVBO(int attrib, VBO vbo)
        {
            vbo.Bind();
            GL.EnableVertexAttribArray(attrib);
            GL.VertexAttribPointer(attrib, vbo.ElementSize, VertexAttribPointerType.Float, false, 0, 0);
        }

        public void Render(int offset = 0)
        {
            if (DisableDepth)
                GL.Disable(EnableCap.DepthTest);

            if (!_indexed)
                GL.DrawArrays(DrawType, offset, NumVerts);
            else
            {
                GL.DrawElements(DrawType, NumVerts, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }

            if (DisableDepth)
                GL.Enable(EnableCap.DepthTest);
        }
    }
}