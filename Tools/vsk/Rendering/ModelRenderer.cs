// http://www.opengl-tutorial.org/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Novalogic._3DI;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace vsk.Rendering
{
    internal class ModelRenderer
    {
        public static Vector3 Up = new Vector3(0, 0, 1);
        public static Vector3 Center = new Vector3(0, 0, 0);
        public static Vector3 Spawn = new Vector3(2, 0, 0);

        private static readonly Color BackColor = Color.FromArgb(51, 127, 178);
        private static readonly Color GridColor = Color.FromArgb(102, 102, 102);

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

        private Camera _camera;
        private readonly File3di _file;

        private readonly Dictionary<int, int> _texHandles = new Dictionary<int, int>();
        private readonly List<VAO> _vaoModels = new List<VAO>();
        private readonly List<int> _vaoTextures = new List<int>();
        private readonly RenderControl _viewport;
        private Light _light;
        private Matrix4 _viewMatrix;
        private ShaderProgram _shaderTextured;
        private ShaderProgram _shaderUniColor;
        private VAO _vaoGrid;

        public ModelRenderer(RenderControl control, File3di file)
        {
            _viewport = control;
            _file = file;

            _viewport.MakeCurrent();
            OnLoad(EventArgs.Empty);

            _viewport.UpdateFrame += OnUpdateFrame;
            _viewport.RenderFrame += OnRenderFrame;
        }

        private static TextureUnit GetTexUnit(int num)
        {
            return TextureUnit.Texture0 + num;
        }

        /// <summary>
        ///     Called after an OpenGL context has been established, but before entering the main loop.
        /// </summary>
        /// <param name="e">Not used.</param>
        private void OnLoad(EventArgs e)
        {
            _shaderUniColor = new ShaderProgram();
            _shaderUniColor.Add(new Shader(ShaderType.VertexShader, @"Rendering/Shaders/vert_uniColor.gls"));
            _shaderUniColor.Add(new Shader(ShaderType.FragmentShader, @"Rendering/Shaders/frag_uniColor.gls"));
            _shaderUniColor.Compile();

            _shaderTextured = new ShaderProgram();
            _shaderTextured.Add(new Shader(ShaderType.VertexShader, @"Rendering/Shaders/vertex.gls"));
            _shaderTextured.Add(new Shader(ShaderType.FragmentShader, @"Rendering/Shaders/fragment.gls"));
            _shaderTextured.Compile();
            _shaderTextured.Use();

            CreateShaders();
            CreateTextures();
            CreateVAO();

            // Other state
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Viewport(0, 0, _viewport.Width, _viewport.Height);
            GL.ClearColor(BackColor);
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

        private static int LoadTexture(Bitmap texture)
        {
            var id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

            var bmpData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.ReadOnly, texture.PixelFormat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            texture.UnlockBits(bmpData);

            return id;
        }

        private void CreateShaders()
        {
            _camera = new Camera(_viewport.ClientSize, Spawn, Center, Up);
            var projectionMatrix = _camera.ProjectionMatrix;
            _viewMatrix = _camera.ViewMatrix;

             _light = new Light(new Vector3(2.0f, 0.0f, 1.0f), new Vector3(1, 1, 1));
            var ambiantLight = new Vector4(0.1f);
            _shaderTextured.SetUniformVec4("ambiantLight", ref ambiantLight);

            double w, l, h;
            _file.Lods[0].Dimensions(out w, out l, out h);
            var maxdim = Math.Max(w, l);

            var scaleMatrix = Matrix4.CreateScale((float) (1/maxdim));
            var worldMatrix = Matrix4.Identity*scaleMatrix;

            _shaderTextured.SetUniformMatrix4("matrixProjection", false, ref projectionMatrix);
            _shaderTextured.SetUniformMatrix4("matrixView", false, ref _viewMatrix);
            _shaderTextured.SetUniformMatrix4("matrixWorld", false, ref worldMatrix);

            var lightPos = _light.Position;
            _shaderTextured.SetUniformVec3("lightPosition", ref lightPos);

            var lightColor = _light.Color;
            _shaderTextured.SetUniformVec3("lightColor", ref lightColor);

            _shaderUniColor.Use();

            scaleMatrix = Matrix4.Identity;
            var color = new Vector4(GridColor.R/(float)byte.MaxValue, GridColor.G / (float)byte.MaxValue, GridColor.B / (float)byte.MaxValue, GridColor.A / (float)byte.MaxValue);
            _shaderUniColor.SetUniformMatrix4("projection_matrix", false, ref projectionMatrix);
            _shaderUniColor.SetUniformMatrix4("modelview_matrix", false, ref _viewMatrix);
            _shaderUniColor.SetUniformMatrix4("scale_matrix", false, ref scaleMatrix);
            _shaderUniColor.SetUniformVec4("vertColor", ref color);

//            CheckError();
        }

        private void CreateVAO()
        {
            var lod = _file.Lods[0];

            foreach (var mesh in BuildMeshes(lod))
            {
                var vao = new VAO(BeginMode.Triangles, mesh.CoordVerts.Count);
                vao.Use();

                var vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordVerts.ToArray(), 4);
                vao.AddVBO(0, vbo);

                vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordNorms.ToArray(), 4);
                vao.AddVBO(1, vbo);

                vbo = new VBO();
                vbo.Buffer(BufferTarget.ArrayBuffer, mesh.CoordTex.ToArray(), 2);
                vao.AddVBO(3, vbo);

                _vaoModels.Add(vao);

                var lodMat = lod.Materials[mesh.MatIndex];
                _vaoTextures.Add(lodMat.TexIndex);
            }
            _vaoGrid = new VAO(BeginMode.Lines, _axisVerts.Length)
            {
                DisableDepth = true
            };
            _vaoGrid.Use();

            var gridVerts = new VBO();
            gridVerts.Buffer(BufferTarget.ArrayBuffer, _axisVerts, 3);
            _vaoGrid.AddVBO(0, gridVerts);

//            CheckError();
        }

        private static IEnumerable<GlMesh> BuildMeshes(File3di.ModelLod lod)
        {
            var meshes = new List<GlMesh>();

            for (var iBones = 0; iBones < lod.SubObjects.Count; iBones++)
            {
                var subObj = lod.SubObjects[iBones];

                //sub.BoneDiffOffset = sub.BoneDiffOffset - SubObjects[sub.parentBone].BoneDiffOffset;

                var foff = lod.FaceOffset(iBones);
                var voff = lod.VecOffset(iBones);

                for (var i = 0; i < subObj.nFaces; i++)
                {
                    var face = lod.Faces[i + foff];

                    var mesh = meshes.Find(g => g.MatIndex == face.MaterialIndex);
                    if (mesh == null)
                    {
                        mesh = new GlMesh {MatIndex = face.MaterialIndex};
                        meshes.Add(mesh);
                    }

                    var vec4 = lod.Vertices[face.Vertex1 + voff];
                    mesh.CoordVerts.Add(vec4 - subObj.BoneOffset);
                    vec4 = lod.Vertices[face.Vertex2 + voff];
                    mesh.CoordVerts.Add(vec4 - subObj.BoneOffset);
                    vec4 = lod.Vertices[face.Vertex3 + voff];
                    mesh.CoordVerts.Add(vec4 - subObj.BoneOffset);

                    mesh.CoordTex.Add(new Vector2(face.tu1/65536.0f, face.tv1/65536.0f));
                    mesh.CoordTex.Add(new Vector2(face.tu2/65536.0f, face.tv2/65536.0f));
                    mesh.CoordTex.Add(new Vector2(face.tu3/65536.0f, face.tv3/65536.0f));

                    vec4 = lod.Normals[face.Normal1];
                    mesh.CoordNorms.Add(vec4);
                    vec4 = lod.Normals[face.Normal2];
                    mesh.CoordNorms.Add(vec4);
                    vec4 = lod.Normals[face.Normal3];
                    mesh.CoordNorms.Add(vec4);
                }
            }

            return meshes;
        }

        private void CreateTextures()
        {
            for (var i = 0; i < _file.Textures.Count; i++)
            {
                _texHandles[i] = LoadTexture(_file.Textures[i].Tex);
            }
        }

        private void BindTexture(ref int textureId, TextureUnit textureUnit, string uniformName)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(_shaderTextured.GetUniform(uniformName), textureUnit - TextureUnit.Texture0);
        }

        private void OnUpdateFrame(object obj, FrameEventArgs e)
        {
            if (!_camera.Think(e.Time))
                return;

            var matrixView = _camera.ViewMatrix;
            _shaderTextured.SetUniformMatrix4("matrixView", false, ref matrixView);
            _shaderUniColor.SetUniformMatrix4("modelview_matrix", false, ref matrixView);
        }


        private void OnRenderFrame(object obj, FrameEventArgs e)
        {
            // clear the last buffer from the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // draw the model
            _shaderTextured.Use();
            for (var i = 0; i < _vaoModels.Count; i++)
            {
                var vao = _vaoModels[i];
                var tex = _texHandles[_vaoTextures[i]];
                BindTexture(ref tex, GetTexUnit(i), "mytexture");
                vao.Use();
                vao.Render();
            }

            // draw the grid lines
            _shaderUniColor.Use();
            _vaoGrid.Use();
            _vaoGrid.Render();

            // finish rendering
            _viewport.SwapBuffers();
        }

        private class GlMesh
        {
            public readonly List<Vector4> CoordNorms = new List<Vector4>();
            public readonly List<Vector2> CoordTex = new List<Vector2>();

            public readonly List<Vector4> CoordVerts = new List<Vector4>();
            public int MatIndex;
        }
    }
}