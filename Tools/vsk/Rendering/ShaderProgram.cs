using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Render
{
    internal class ShaderProgram
    {
        private int _handle = -1;
        public Shader FragmentShader;
        public Shader VertexShader;

        public void Add(Shader shader)
        {
            switch (shader.Type)
            {
                case ShaderType.VertexShader:
                    VertexShader = shader;
                    break;
                case ShaderType.FragmentShader:
                    FragmentShader = shader;
                    break;
                default:
                    throw new Exception("Tride to add wrong shader type!");
            }
        }

        public void Compile()
        {
            _handle = GL.CreateProgram();

            if (VertexShader != null)
                GL.AttachShader(_handle, VertexShader.Handle);

            if (FragmentShader != null)
                GL.AttachShader(_handle, FragmentShader.Handle);

            GL.LinkProgram(_handle);

            int compiled;
            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out compiled);
            if (compiled != 1)
            {
                throw new Exception(GL.GetProgramInfoLog(_handle));
            }
        }

        public void Use()
        {
            if (_handle != -1)
                GL.UseProgram(_handle);
            else
                throw new Exception("Shader has no valid handle!");
        }

        public int GetUniform(string name)
        {
            if (_handle == -1)
                throw new Exception("Shader has no valid handle!");

            var ret = GL.GetUniformLocation(_handle, name);
            if (ret == -1)
                throw new Exception("Could not get uniform!");
            return ret;
        }

        public void SetUniformMatrix4(string uniformName, bool transpose, ref Matrix4 matrix)
        {
            Use();
            var uniformId = GetUniform(uniformName);
            GL.UniformMatrix4(uniformId, transpose, ref matrix);
        }

        public void SetUniformVec4(string uniformName, ref Vector4 vector)
        {
            Use();
            var uniformId = GetUniform(uniformName);
            GL.Uniform4(uniformId, vector);
        }
    }
}