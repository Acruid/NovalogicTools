using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    internal class ShaderProgram
    {
        private int _handle = -1;
        private Shader _fragmentShader;
        private Shader _vertexShader;
        private readonly Dictionary<string, int> _uniformCache = new Dictionary<string, int>();

        public void Add(Shader shader)
        {
            _uniformCache.Clear();
            switch (shader.Type)
            {
                case ShaderType.VertexShader:
                    _vertexShader = shader;
                    break;
                case ShaderType.FragmentShader:
                    _fragmentShader = shader;
                    break;
                default:
                    throw new Exception("Tride to add wrong shader type!");
            }
        }

        public void Compile()
        {
            _uniformCache.Clear();
            _handle = GL.CreateProgram();

            if (_vertexShader != null)
                GL.AttachShader(_handle, _vertexShader.Handle);

            if (_fragmentShader != null)
                GL.AttachShader(_handle, _fragmentShader.Handle);

            GL.LinkProgram(_handle);

            int compiled;
            GL.GetProgram(_handle, ProgramParameter.LinkStatus, out compiled);
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

            int result;
            if (_uniformCache.TryGetValue(name, out result))
                return result;

            result = GL.GetUniformLocation(_handle, name);
            if (result == -1)
                throw new Exception("Could not get uniform!");
            _uniformCache.Add(name, result);
            return result;
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

        public void SetUniformVec3(string uniformName, ref Vector3 vector)
        {
            Use();
            var uniformId = GetUniform(uniformName);
            GL.Uniform3(uniformId, vector);
        }
    }
}