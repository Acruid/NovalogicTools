using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;

namespace Novalogic._3DI
{
    /// <summary>
    ///     Used for interacting with a 3DI file.
    /// </summary>
    public class File3di
    {
        private File3di(BinaryReader reader, FileVersion version)
        {
            if (version != FileVersion.V8)
                throw new NotSupportedException();

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var header = reader.ReadBytes(Header.STRUCT_SIZE).ToStruct<Header>();

            Textures = new List<ModelTexture>(header.TextureCount);
            for (var i = 0; i < header.TextureCount; i++)
            {
                var newTex = new ModelTexture(reader, header.Signature);
                Textures.Add(newTex);
            }

            Lods = new List<ModelLod>();
            for (var i = 0; i < header.LodInfo.Count; i++)
            {
                var lod = new ModelLod(reader, header.Signature);
                Lods.Add(lod);
            }
        }

        /// <summary>
        /// List of textures inside of the file.
        /// </summary>
        public List<ModelTexture> Textures { get; }

        /// <summary>
        /// List of lods inside of the file.
        /// </summary>
        public List<ModelLod> Lods { get; }

        /// <summary>
        ///     Opens a 3DI file.
        /// </summary>
        /// <param name="file">The 3DI file path.</param>
        /// <returns>A Parsed 3DI file.</returns>
        public static File3di Open(FileInfo file)
        {
            if (!file.Exists)
                return null;

            using (var reader = new BinaryReader(file.OpenRead()))
            {
                return Open(reader);
            }
        }

        /// <summary>
        ///     Opens a 3DI file.
        /// </summary>
        /// <param name="fileContents">The contents of a 3DI file.</param>
        /// <returns></returns>
        public static File3di Open(byte[] fileContents)
        {
            using (var reader = new BinaryReader(new MemoryStream(fileContents)))
            {
                return Open(reader);
            }
        }

        /// <summary>
        ///     Opens a 3DI file.
        /// </summary>
        /// <param name="reader">A BinaryReader </param>
        /// <returns></returns>
        private static File3di Open(BinaryReader reader)
        {
            // Check signature
            var version = (FileVersion) reader.ReadUInt32();
            return new File3di(reader, version);
        }

        private static string CleanString(string str)
        {
            str = str.TrimStart('\0');
            var index = str.IndexOf("\0", StringComparison.Ordinal);
            if (index > 0)
                str = str.Remove(index);
            return str;
        }

        #region BinaryStructs

        // ReSharper disable UnassignedGetOnlyAutoProperty

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct Header
        {
            public const int STRUCT_SIZE = 128;

            public FileVersion Signature { get; }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] private readonly byte[] name;
            public string Name => CleanString(Utility.TextEncode.GetString(name));

            private readonly uint GAP_0;

            public HeaderLodInfo LodInfo { get; }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17*4)] private readonly byte[] GAP_1;

            public int TextureCount { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct HeaderLodInfo
        {
            private const int STRUCT_SIZE = 20;

            public uint Count { get; }
            public uint DistHigh { get; }
            public uint DistMedium { get; }
            public uint DistLow { get; }
            public uint DistTiny { get; }
            public LodRenderType_V8 RendHigh { get; }
            public LodRenderType_V8 RendMedium { get; }
            public LodRenderType_V8 RendLow { get; }
            public LodRenderType_V8 RendTiny { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = ARRAY_SIZE, Pack = 1)]
        private struct ModelTexHeader
        {
            public const int ARRAY_SIZE = 52;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)] private readonly byte[] name;
            public string Name => CleanString(Utility.TextEncode.GetString(name));

            public readonly int _bmSize;
            public readonly ushort Index;
            public readonly ushort _flags;
            public readonly ushort _bmWidth;
            public readonly ushort _bmHeight;
            private readonly uint PTR_BMLines;
            private readonly uint PTR_Palette;
            private readonly uint PTR_PaletteEnd;
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public struct ModelFace
        {
            public const int STRUCT_SIZE = 72;

            private readonly short null0;
            private readonly short SurfaceIndex;
            public int tu1;
            public int tu2;
            public int tu3;
            public int tv1;
            public int tv2;
            public int tv3;

            public short Vertex1;
            public short Vertex2;
            public short Vertex3;
            public short Normal1;
            public short Normal2;
            public short Normal3;

            private readonly int Distance;
            private readonly int xMin;
            private readonly int xMax;
            private readonly int yMin;
            private readonly int yMax;
            private readonly int zMin;
            private readonly int zMax;

            public int MaterialIndex; //v,r index of material for faces
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public struct ModelMaterial
        {
            public const int STRUCT_SIZE = 0x78;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] name;
            public string Name => CleanString(Utility.TextEncode.GetString(name));

            private readonly byte BitFlags; //v,r flags for some type of setup

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] private readonly byte[] pad0; //v,n padding for the bitflags

            private readonly uint gap14; //u

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] private readonly uint[] null0; //

            public byte IndexG; // index of texture to use
            private readonly byte IndexB; // index of texture to use
            private readonly byte IndexW; // index of texture to use
            private readonly byte IndexA; //

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] private readonly uint[] null1; //


            public byte TexIndex => IndexG;
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public class ModelSubObject
        {
            public const int STRUCT_SIZE = 112;

            private int GAP_0;
            public int nVerts; //v,r number of verts in the subObject
            private int PTR_VertGroup; //v,w ptr to vert data for this subObject
            public int nFaces; //v,r number of faces in sub object
            private int PTR_FaceGroup; //v,w ptr to face data of sub object
            private int nNormals; //w,r ''
            private int PTR_NormGroup; //v,w ''
            private int nColVolumes; //v,r ''
            private int PTR_ColVolumes; //v,w ''
            
            // ignore this if(lodheader.Flags & 1 == false)
            public int parentBone; //v,r parent bone this is attached to
            private int diffXoff; //v,w VecXOff - parentBone.VecXoff
            private int diffYoff; //v,w VecYOff - parentBone.VecYoff
            private int diffZoff; //v,w VecZOff - parentBone.VecZoff

            //v,r if(lodheader.Flags & 1)foreach vert in group, vec.x -= (VecXoff >> 8)
            private int VecXoff; //v,r
            private int VecYoff; //v,r same as above for y
            private int VecZoff; //v,r same as above for z

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)] private int[] GAP_1;
            
            public Vector4 BoneOffset => new Vector4
            {
                X = (byte) VecXoff >> 8,
                Y = (byte) VecYoff >> 8,
                Z = (byte) VecZoff >> 8,
                W = 0
            };

            public Vector3 BoneDiffOffset => new Vector3
            {
                X = (byte) diffXoff,
                Y = (byte) diffYoff,
                Z = (byte) diffZoff
            };
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct ModelLodHeader
        {
            public const int STRUCT_SIZE = 192;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] private readonly int[] null0;
            private readonly int Flags; //v,r if(Flags & 1) offset verts in Bones
            private readonly int length;
            private readonly int PTR_ModelInfo; //v,w ptr to all of the model info after header

            private readonly int SphereRadius;
            private readonly int CircleRadius;
            private readonly int zTotal;
            private readonly int xMin;
            private readonly int xMax;
            private readonly int yMin;
            private readonly int yMax;
            private readonly int zMin;
            private readonly int zMax;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] private readonly int[] null2;

            public readonly int nVertices; //v,r number of verts in lod mesh
            private readonly int null3; //v,w number of verts in lod mesh
            public readonly int nNormals; //v,r number of normals in lod mesh
            private readonly int null4; //v,w ptr to start of normals in-mem
            public readonly int nFaces; //v,r number of faces of mesh
            private readonly int null5; //v,w ptr to start of faces in-mem
            public readonly int nSubObjects; //v,r number of sub objects
            private readonly int null6; //v,w ptr to start of sub objects
            public readonly int nPartAnims; //v,r
            private readonly int null7; //v,w
            public readonly int nMaterials; //v,r
            private readonly int null8; //v,w
            public readonly int nColPlanes; //v,r
            private readonly int null9; //v,w
            public readonly int nColVolumes; //v,r
            private readonly int null10; //v,w
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct ModelBoneAnim
        {
            public const int STRUCT_SIZE = 0x0C;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] private readonly byte[] GAP_0;
        }

        // ReSharper restore UnassignedGetOnlyAutoProperty

        public class ModelLod
        {
            private ModelLodHeader header;

            public ModelLod(BinaryReader reader, FileVersion version)
            {
                if (version != FileVersion.V8)
                    throw new NotSupportedException();

                Deserialize(reader);
            }

            public List<Vector4> Vertices { get; set; }
            public List<Vector4> Normals { get; set; }
            public List<ModelFace> Faces { get; set; }
            public List<ModelSubObject> SubObjects { get; set; }
            public List<ModelMaterial> Materials { get; set; }

            public void Serialize(BinaryReader reader)
            {
                throw new NotSupportedException();
            }

            private void Deserialize(BinaryReader reader)
            {
                header = reader.ReadBytes(ModelLodHeader.STRUCT_SIZE).ToStruct<ModelLodHeader>();

                Vertices = new List<Vector4>(header.nVertices);
                for (var i = 0; i < header.nVertices; i++)
                {
                    var vec = new Vector4
                    {
                        X = reader.ReadInt16(),
                        Y = reader.ReadInt16(),
                        Z = reader.ReadInt16(),
                        W = reader.ReadInt16()
                    };
                    Vertices.Add(vec);
                }

                Normals = new List<Vector4>(header.nNormals);
                for (var i = 0; i < header.nNormals; i++)
                {
                    var vec = new Vector4
                    {
                        X = reader.ReadInt16(),
                        Y = reader.ReadInt16(),
                        Z = reader.ReadInt16(),
                        W = reader.ReadInt16()
                    };
                    Normals.Add(vec);
                }

                Faces = new List<ModelFace>(header.nFaces);
                for (var i = 0; i < header.nFaces; i++)
                {
                    var face = reader.ReadBytes(ModelFace.STRUCT_SIZE).ToStruct<ModelFace>();
                    Faces.Add(face);
                }

                SubObjects = new List<ModelSubObject>(header.nSubObjects);
                for (var i = 0; i < header.nSubObjects; i++)
                {
                    var obj = reader.ReadBytes(ModelSubObject.STRUCT_SIZE).ToStruct<ModelSubObject>();
                    SubObjects.Add(obj);
                }

                for (var i = 0; i < header.nPartAnims; i++)
                {
                    var anm = reader.ReadBytes(0x0C).ToStruct<ModelBoneAnim>();
                }

                //TODO: Read ColPlanes
                reader.BaseStream.Position += 0x08*header.nColPlanes;

                //TODO: Read ColVolumes
                reader.BaseStream.Position += 0x50*header.nColVolumes;

                Materials = new List<ModelMaterial>(header.nMaterials);
                for (var i = 0; i < header.nMaterials; i++)
                {
                    var mat = reader.ReadBytes(ModelMaterial.STRUCT_SIZE).ToStruct<ModelMaterial>();
                    Materials.Add(mat);
                }
            }

            public void Dimensions(out double width, out double length, out double height)
            {
                double maxx, minx, maxy, miny, maxz, minz;
                maxx = maxy = maxz = minx = miny = minz = 0;
                foreach (var vert in Vertices)
                {
                    if (vert.X > maxx) maxx = vert.X;
                    if (vert.Y > maxy) maxy = vert.Y;
                    if (vert.Z > maxz) maxz = vert.Z;
                    if (vert.X < minx) minx = vert.X;
                    if (vert.Y < miny) miny = vert.Y;
                    if (vert.Z < minz) minz = vert.Z;
                }
                width = maxx - minx;
                length = maxy - miny;
                height = maxz - minz;
            }

            public int FaceOffset(int n)
            {
                if (n <= 0)
                    return 0;

                var off = 0;
                for (var i = 0; i < n; i++)
                    off += SubObjects[i].nFaces;

                return off;
            }

            public int VecOffset(int n)
            {
                if (n <= 0)
                    return 0;

                var off = 0;
                for (var i = 0; i < n; i++)
                    off += SubObjects[i].nVerts;

                return off;
            }
        }

        public class ModelTexture
        {
            private ModelTexHeader header;

            public ModelTexture(BinaryReader reader, FileVersion version)
            {
                if (version != FileVersion.V8)
                    throw new NotSupportedException();

                Deserialize(reader);
            }

            public Bitmap Tex { get; private set; }

            public void Serialize(BinaryReader reader)
            {
                throw new NotSupportedException();
            }

            private void Deserialize(BinaryReader reader)
            {
                header = reader.ReadBytes(ModelTexHeader.ARRAY_SIZE).ToStruct<ModelTexHeader>();

                var bmLines = reader.ReadBytes(header._bmSize);
                var palBuf = reader.ReadBytes(256*4);

                Tex = GenerateTexture(header, bmLines, palBuf);
            }

            private static Bitmap GenerateTexture(ModelTexHeader header, byte[] scanLines, byte[] palette)
            {
                var bmpTex = new Bitmap(header._bmWidth, header._bmHeight, PixelFormat.Format32bppArgb);
                var numPixels = header._bmWidth*header._bmHeight;
                var stride = header._bmSize/numPixels;

                var bmpData = bmpTex.LockBits(new Rectangle(0, 0, bmpTex.Width, bmpTex.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                var bytes = bmpData.Stride*bmpData.Height;
                var texBuffer = new byte[bytes];

                //WARNING: This only works with powers of 2 sized images
                for (var i = 0; i < numPixels; i++)
                {
                    var index = scanLines[i*stride + 0]*4;
                    texBuffer[i*4 + 3] = stride == 2 ? scanLines[i*stride + 1] : (byte) 255; // A
                    texBuffer[i*4 + 2] = palette[index + 2]; // R
                    texBuffer[i*4 + 1] = palette[index + 1]; // G
                    texBuffer[i*4 + 0] = palette[index + 0]; // B
                }

                Marshal.Copy(texBuffer, 0, bmpData.Scan0, bytes);
                bmpTex.UnlockBits(bmpData);
                return bmpTex;
            }

            public static void ExportPng(Bitmap texture, string fileName)
            {
                using (var memory = new MemoryStream())
                {
                    using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        texture.Save(memory, ImageFormat.Png);
                        var bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public enum FileVersion : uint
        {
            ERROR = 0,
            V8 = 0x08494433 //{ '3', 'D', 'I', 0x8 }
        }

        private enum LodRenderType_V8 : uint
        {
            NONE = 0x0,
            GENERIC = 0x676E7263 //"crng"
        }

        #endregion
    }
}