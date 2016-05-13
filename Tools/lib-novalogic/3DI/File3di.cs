using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Novalogic._3DI
{
    /// <summary>
    /// Used for interacting with a 3DI file.
    /// </summary>
    public class File3di
    {
        public List<ModelTexture> Textures { get; }
        public List<ModelLod> Lods { get; }

        /// <summary>
        /// Opens a 3DI file.
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
        /// Opens a 3DI file.
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
        /// Opens a 3DI file.
        /// </summary>
        /// <param name="reader">A BinaryReader </param>
        /// <returns></returns>
        private static File3di Open(BinaryReader reader)
        {
            // Check signature
            var version = (FileVersion)reader.ReadUInt32();
            return new File3di(reader, version);
        }

        private File3di(BinaryReader reader, FileVersion version)
        {
            if(version != FileVersion.V8)
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
            for (var i = 0; i < header.LodInfo.Count;i++)
            {
                var lod = new ModelLod(reader, header.Signature);
                Lods.Add(lod);
            }
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

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            private readonly Byte[] name;
            public String Name => CleanString(Utility.TextEncode.GetString(name));

            private readonly UInt32 GAP_0;

            public HeaderLodInfo LodInfo { get; }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17*4)]
            private readonly Byte[] GAP_1;

            public Int32 TextureCount { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct HeaderLodInfo
        {
            private const int STRUCT_SIZE = 20;

            public UInt32 Count { get; }
            public UInt32 DistHigh { get; }
            public UInt32 DistMedium { get; }
            public UInt32 DistLow { get; }
            public UInt32 DistTiny { get; }
            public LodRenderType_V8 RendHigh { get; }
            public LodRenderType_V8 RendMedium { get; }
            public LodRenderType_V8 RendLow { get; }
            public LodRenderType_V8 RendTiny { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = ARRAY_SIZE, Pack = 1)]
        private struct TexHeader_V8
        {
            public const int ARRAY_SIZE = 52;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            private readonly Byte[] name;
            public String Name => CleanString(Utility.TextEncode.GetString(name));

            public int _bmSize;
            public UInt16 Index;
            public UInt16 _flags;
            public UInt16 _bmWidth;
            public UInt16 _bmHeight;
            private UInt32 PTR_BMLines;
            private UInt32 PTR_Palette;
            private UInt32 PTR_PaletteEnd;
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public struct ModelFace
        {
            public const int STRUCT_SIZE = 72;

            short null0;
            short SurfaceIndex;
            int tu1;
            int tu2;
            int tu3;
            int tv1;
            int tv2;
            int tv3;

            short Vertex1;
            short Vertex2;
            short Vertex3;
            short Normal1;
            short Normal2;
            short Normal3;

            int Distance;
            int xMin;
            int xMax;
            int yMin;
            int yMax;
            int zMin;
            int zMax;

            int MaterialIndex; //v,r index of material for faces
        }
        
        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public struct ModelMaterial
        {
            public const int STRUCT_SIZE = 0x78;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Byte[] name;
            public string Name =>CleanString(Utility.TextEncode.GetString(name));

            private Byte BitFlags; //v,r flags for some type of setup

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            private Byte[] pad0; //v,n padding for the bitflags

            private UInt32 gap14; //u

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            private UInt32[] null0; //

            public Byte IndexG; // index of texture to use
            private Byte IndexB; // index of texture to use
            private Byte IndexW; // index of texture to use
            private Byte IndexA; //

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private UInt32[] null1; //


            public byte TexIndex => IndexG;
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        public struct ModelSubObject
        {
            public const int STRUCT_SIZE = 112;

            int GAP_0;
            int nVerts;        //v,r number of verts in the subObject
            int PTR_VertGroup; //v,w ptr to vert data for this subObject
            int nFaces;        //v,r number of faces in sub object
            int PTR_FaceGroup; //v,w ptr to face data of sub object
            int nNormals;      //w,r ''
            int PTR_NormGroup; //v,w ''
            int nColVolumes;   //v,r ''
            int PTR_ColVolumes;//v,w ''

            // ignore this if(lodheader.Flags & 1 == false)
            int parentBone;    //v,r parent bone this is attached to
            int diffXoff;      //v,w VecXOff - parentBone.VecXoff
            int diffYoff;      //v,w VecYOff - parentBone.VecYoff
            int diffZoff;      //v,w VecZOff - parentBone.VecZoff

            //v,r if(lodheader.Flags & 1)foreach vert in group, vec.x -= (VecXoff >> 8)
            int VecXoff;       //v,r
            int VecYoff;       //v,r same as above for y
            int VecZoff;       //v,r same as above for z

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            int[] GAP_1;
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct ModelLodHeader
        {
            public const int STRUCT_SIZE = 192;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            int[] null0;
            int Flags;                 //v,r if(Flags & 1) offset verts in Bones
            int length;
            int PTR_ModelInfo;         //v,w ptr to all of the model info after header

            int SphereRadius;
            int CircleRadius;
            int zTotal;
            int xMin;
            int xMax;
            int yMin;
            int yMax;
            int zMin;
            int zMax;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            int[] null2;

            public int nVertices;     //v,r number of verts in lod mesh
            int null3;         //v,w number of verts in lod mesh
            public int nNormals;      //v,r number of normals in lod mesh
            int null4;         //v,w ptr to start of normals in-mem
            public int nFaces;        //v,r number of faces of mesh
            int null5;         //v,w ptr to start of faces in-mem
            public int nSubObjects;   //v,r number of sub objects
            int null6;         //v,w ptr to start of sub objects
            public int nPartAnims;    //v,r
            int null7;         //v,w
            public int nMaterials;    //v,r
            int null8;         //v,w
            public int nColPlanes;    //v,r
            int null9;         //v,w
            public int nColVolumes;   //v,r
            int null10;        //v,w
        }

        [StructLayout(LayoutKind.Sequential, Size = STRUCT_SIZE, Pack = 1)]
        private struct ModelBoneAnim
        {
            public const int STRUCT_SIZE = 0x0C;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            private byte[] GAP_0;
        }

        // ReSharper restore UnassignedGetOnlyAutoProperty

        public class ModelLod
        {
            public List<Vector4> Vertices { get; set; }
            public List<Vector4> Normals { get; set; }
            public List<ModelFace> Faces { get; set; }
            public List<ModelSubObject> SubObjects { get; set; }
            public List<ModelMaterial> Materials { get; set; }

            private ModelLodHeader header;

            public ModelLod(BinaryReader reader, FileVersion version)
            {
                if (version != FileVersion.V8)
                    throw new NotSupportedException();

                Deserialize(reader);
            }

            public void Serialize(BinaryReader reader)
            {
                throw new NotSupportedException();
            }

            public void Deserialize(BinaryReader reader)
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
                reader.BaseStream.Position += 0x08 * header.nColPlanes;

                //TODO: Read ColVolumes
                reader.BaseStream.Position += 0x50 * header.nColVolumes;

                Materials = new List<ModelMaterial>(header.nMaterials);
                for (var i = 0; i < header.nMaterials; i++)
                {
                    Materials.Add(new ModelMaterial());
                }
            }
        }

        public class ModelTexture
        {
            private TexHeader_V8 header;
            public Bitmap Tex { get; private set; }

            public ModelTexture(BinaryReader reader, FileVersion version)
            {
                if(version != FileVersion.V8)
                    throw new NotSupportedException();

                Deserialize(reader);
            }

            public void Serialize(BinaryReader reader)
            {
                throw new NotSupportedException();
            }

            private void Deserialize(BinaryReader reader)
            {
                header = reader.ReadBytes(TexHeader_V8.ARRAY_SIZE).ToStruct<TexHeader_V8>();

                var bmLines = reader.ReadBytes(header._bmSize);
                var palBuf = reader.ReadBytes(256 * 4);

                Tex = GenerateTexture(header, bmLines, palBuf);
            }

            private static Bitmap GenerateTexture(TexHeader_V8 header, byte[] scanLines, byte[] palette)
            {
                var bmpTex = new Bitmap(header._bmWidth, header._bmHeight, PixelFormat.Format32bppArgb);
                var numPixels = header._bmWidth * header._bmHeight;
                var stride = header._bmSize / numPixels;

                var bmpData = bmpTex.LockBits(new Rectangle(0, 0, bmpTex.Width, bmpTex.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                var bytes = bmpData.Stride * bmpData.Height;
                var texBuffer = new byte[bytes];

                //WARNING: This only works with powers of 2 sized images
                for (var i = 0; i < numPixels; i++)
                {
                    var index = scanLines[i * stride + 0] * 4;
                    texBuffer[i * 4 + 3] = stride == 2 ? scanLines[i * stride + 1] : (byte)255; // A
                    texBuffer[i * 4 + 2] = palette[index + 2]; // R
                    texBuffer[i * 4 + 1] = palette[index + 1]; // G
                    texBuffer[i * 4 + 0] = palette[index + 0]; // B
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
            GENERIC = 0x676E7263, //"crng"
        }
        
        #endregion
    }
}
