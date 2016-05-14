// This code was written for the OpenTK library and has been released
// to the Public Domain.
// It is provided "as is" without express or implied warranty of any kind.

// http://www.opengl-tutorial.org/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
//using Render.Loader;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Render
{
    public class HelloGL3
    {
        public static Vector3 Up = new Vector3(0, 0, 1);
        public static Vector3 Center = new Vector3(0, 0, 0);
        public static Vector3 Spawn = new Vector3(-2.0f, 0, 1.0f);

        private readonly Vector3[] _axisVerts =
        {
            // Axis
            new Vector3(0, 0, 0),
            new Vector3(0.25f, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0.25f, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0.25f),

            // Grid Border
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0)
        };

        private readonly Camera _camera = new Camera();
        // The callback delegate must be stored to avoid GC
        private readonly DebugProc _debugCallbackInstance = DebugCallback;
        private readonly Dictionary<int, int> _texHandles = new Dictionary<int, int>();
//        private int _vboIndicies;

        private readonly List<VAO> _vaoModel = new List<VAO>();
        private Matrix4 _modelviewMatrix;
//        private Model3D _obj;
        private ShaderProgram _shaderTextured;
        private ShaderProgram _shaderUniColor;
        private VAO _vaoGrid;

        private HelloGL3()
            : base(1366, 768,
                new GraphicsMode(), "OpenGL 3 Example", 0,
                DisplayDevice.Default, 3, 2,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id,
            DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            var msg = Marshal.PtrToStringAnsi(message);
            Console.WriteLine("[GL] {0}; {1}; {2}; {3}; {4}\n",
                source, type, id, severity, msg);
        }

        private static void CheckError()
        {
            var errorCode = GL.GetError();

            if (errorCode == ErrorCode.NoError)
                return;

            string error;
            var description = "No Description";

            // Decode the error code
            switch (errorCode)
            {
                case ErrorCode.InvalidEnum:
                {
                    error = "GL_INVALID_ENUM";
                    description = "An unacceptable value has been specified for an enumerated argument";
                    break;
                }

                case ErrorCode.InvalidValue:
                {
                    error = "GL_INVALID_VALUE";
                    description = "A numeric argument is out of range";
                    break;
                }

                case ErrorCode.InvalidOperation:
                {
                    error = "GL_INVALID_OPERATION";
                    description = "The specified operation is not allowed in the current state";
                    break;
                }

                case ErrorCode.StackOverflow:
                {
                    error = "GL_STACK_OVERFLOW";
                    description = "This command would cause a stack overflow";
                    break;
                }

                case ErrorCode.StackUnderflow:
                {
                    error = "GL_STACK_UNDERFLOW";
                    description = "This command would cause a stack underflow";
                    break;
                }

                case ErrorCode.OutOfMemory:
                {
                    error = "GL_OUT_OF_MEMORY";
                    description = "there is not enough memory left to execute the command";
                    break;
                }

                case ErrorCode.InvalidFramebufferOperationExt:
                {
                    error = "GL_INVALID_FRAMEBUFFER_OPERATION_EXT";
                    description = "The object bound to FRAMEBUFFER_BINDING_EXT is not \"framebuffer complete\"";
                    break;
                }
                default:
                {
                    error = errorCode.ToString();
                    break;
                }
            }

            // Log the error
            throw new Exception("An internal OpenGL call failed: " + error + " (" + description + ")");
        }

        private static TextureUnit GetTexUnit(int num)
        {
            return (TextureUnit.Texture0 + num);
        }

        /// <summary>
        ///     Called after an OpenGL context has been established, but before entering the main loop.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;

            GL.DebugMessageCallback(_debugCallbackInstance, IntPtr.Zero);

            _shaderUniColor = new ShaderProgram();
            _shaderUniColor.Add(new Shader(ShaderType.VertexShader, @"Shaders/vert_uniColor.gls"));
            _shaderUniColor.Add(new Shader(ShaderType.FragmentShader, @"Shaders/frag_uniColor.gls"));
            _shaderUniColor.Compile();

//            TerrainDef terrainDef = TrnLoader.Load(@"Data/DFG3/DFG3.TRN");

//            _obj = Obj.Create(new FileInfo(@"Models\debug2\cube.obj"));
            _obj = _3DI.Create(new FileInfo(@"Models/BSFLGBLU.3DI"));
//            _obj = _3DI.Create(new FileInfo(@"Models/TANKJAX.3DI"));
//            _obj = _3DI.Create(new FileInfo(@"Models/BARREL.3DI"));
//            _obj = _3DI.Create(new FileInfo(@"Models/ADWL.3DI"));

            _obj.ExportObj();

            _shaderTextured = new ShaderProgram();

            _shaderTextured.Add(new Shader(ShaderType.VertexShader, @"Shaders/vertex.gls"));
            _shaderTextured.Add(new Shader(ShaderType.FragmentShader, @"Shaders/fragment.gls"));

            _shaderTextured.Compile();
            _shaderTextured.Use();

            CreateShaders();
            CreateVAO();

            // Other state
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
//            GL.Enable(EnableCap.CullFace);

//            GL.PointSize(3);
//            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.Viewport(0, 0, Width, Height);
            GL.ClearColor(Color.MidnightBlue);

            for (var i = 0; i < _obj.Meshes.Count; i++)
            {
                var texName = _obj.Meshes[i].Material;
                if (!string.IsNullOrEmpty(texName))
                {
                    _texHandles[i] = LoadTexture(_obj.Materials[texName].diffuseMap);
                }
                else
                {
                    _texHandles[i] = LoadTexture(@"Models/error.jpg");
                }
            }
        }

        /// <summary>
        ///     Loads a texture from a file into OpenGL.
        ///     Make sure the dimensions are a power of two (2^n)!
        /// </summary>
        /// <param name="filename">Filename of the texture.</param>
        /// <returns>Pointer to the texture in gpu memory.</returns>
        private static int LoadTexture(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            var id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

            var bmp = new Bitmap(filename);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return id;
        }

        private void CreateShaders()
        {
            // generate values for shader uniform variables
            Matrix4 projectionMatrix;
            var aspectRatio = ClientSize.Width/(float) (ClientSize.Height);
            Matrix4.CreatePerspectiveFieldOfView((float) Math.PI/4, aspectRatio, 1, 1000, out projectionMatrix);
            _modelviewMatrix = Matrix4.LookAt(Spawn, Center, Up);

            // We do some heuristics to try to auto-zoom to a reasonable distance.  And it generally works!
            double w, l, h;
            _obj.Meshes[0].Dimensions(out w, out l, out h);
            Console.WriteLine("Model dimensions: {0} x {1} x {2}", w, l, h);
            var maxdim = Math.Max(Math.Max(w, l), h);
//            _viewDist = (float)(maxdim * 2);

            var scaleMatrix = Matrix4.CreateScale((float) (1/maxdim));

            _shaderTextured.SetUniformMatrix4("projection_matrix", false, ref projectionMatrix);
            _shaderTextured.SetUniformMatrix4("modelview_matrix", false, ref _modelviewMatrix);
            _shaderTextured.SetUniformMatrix4("scale_matrix", false, ref scaleMatrix);

            _shaderUniColor.Use();

            scaleMatrix = Matrix4.CreateScale(1.0f);
            var color = new Vector4(1, 0, 0, 1);
            _shaderUniColor.SetUniformMatrix4("projection_matrix", false, ref projectionMatrix);
            _shaderUniColor.SetUniformMatrix4("modelview_matrix", false, ref _modelviewMatrix);
            _shaderUniColor.SetUniformMatrix4("scale_matrix", false, ref scaleMatrix);
            _shaderUniColor.SetUniformVec4("vertColor", ref color);

            CheckError();
        }

        private void CreateVAO()
        {
            foreach (var mesh in _obj.Meshes)
            {
                var vao = new VAO(PrimitiveType.Triangles, mesh.CoordVerts.Count);
                vao.Use();

                var vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordVerts.ToArray(), 3, BufferUsageHint.StaticDraw);
                vao.AddVBO(0, vbo);

                vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordNorm.ToArray(), 3, BufferUsageHint.StaticDraw);
                vao.AddVBO(1, vbo);

                vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordTex.ToArray(), 2, BufferUsageHint.StaticDraw);
                vao.AddVBO(3, vbo);

                _vaoModel.Add(vao);
            }

            _vaoGrid = new VAO(PrimitiveType.Lines, _axisVerts.Length)
            {
                DisableDepth = true
            };
            _vaoGrid.Use();

            var gridVerts = new VBO();
            gridVerts.Buffer(BufferTarget.ArrayBuffer, _axisVerts, 3, BufferUsageHint.StaticDraw);
            _vaoGrid.AddVBO(0, gridVerts);

            CheckError();
        }

        private void BindTexture(ref int textureId, TextureUnit textureUnit, string uniformName)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(_shaderTextured.GetUniform(uniformName), textureUnit - TextureUnit.Texture0);
        }

        /// <summary>
        ///     Called when the frame is updated.
        /// </summary>
        /// <param name="e">Contains information necessary for frame updating.</param>
        /// <remarks>
        ///     Subscribe to the <see cref="E:OpenTK.GameWindow.UpdateFrame" /> event instead of overriding this method.
        /// </remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[Key.Escape])
                Exit();

            if (!_camera.Think(e.Time))
                return;

            var modelView = _camera.ModelView;
            _shaderTextured.SetUniformMatrix4("modelview_matrix", false, ref modelView);
            _shaderUniColor.SetUniformMatrix4("modelview_matrix", false, ref modelView);
        }

        /// <summary>
        ///     Called when the frame is rendered.
        /// </summary>
        /// <param name="e">Contains information necessary for frame rendering.</param>
        /// <remarks>
        ///     Subscribe to the <see cref="E:OpenTK.GameWindow.RenderFrame" /> event instead of overriding this method.
        /// </remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // clear the last buffer from the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // draw the model
            _shaderTextured.Use();
            for (var i = 0; i < _vaoModel.Count; i++)
            {
                var vao = _vaoModel[i];
                var tex = _texHandles[i];
                BindTexture(ref tex, GetTexUnit(i), "mytexture");
                vao.Use();
                vao.Render();
            }

            // draw the grid lines
            _shaderUniColor.Use();
            _vaoGrid.Use();
            _vaoGrid.Render();

            // finish rendering
            SwapBuffers();
        }
    }
}