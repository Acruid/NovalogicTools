using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace vsk.Rendering
{
    internal class Shader
    {
        public Shader(ShaderType type, string filepath)
        {
            string shaderSource;
            using (var sr = new StreamReader(filepath))
            {
                shaderSource = sr.ReadToEnd();
                sr.Close();
            }

            Handle = GL.CreateShader(type);
            Type = type;
            GL.ShaderSource(Handle, shaderSource);
            GL.CompileShader(Handle);

            int compiled;
            GL.GetShader(Handle, ShaderParameter.CompileStatus, out compiled);
            if (compiled != 1)
            {
                throw new Exception(GL.GetShaderInfoLog(Handle));
            }
        }

        public int Handle { get; }
        public ShaderType Type { get; private set; }
    }
}