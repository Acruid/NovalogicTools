using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Novalogic.TGA
{
    public static class TgaConvert
    {
        // Reference:
        // http://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf
        // http://www.gamers.org/dEngine/quake3/TGA.txt

        public static Bitmap LoadTga(byte[] fileBytes)
        {
            if (fileBytes.Length < 0x80)
                return null;

            using (var stream = new BinaryReader(new MemoryStream(fileBytes)))
            {
                return LoadTga(stream);
            }
        }

        private static Bitmap LoadTga(BinaryReader stream)
        {
            
            // Determine if origional or new TGA
            stream.BaseStream.Seek(-26, SeekOrigin.End);
            var footer = stream.ReadBytes(26).ToStruct<TgaFooter>();
            
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            var header = stream.ReadBytes(18).ToStruct<TgaHeader>();

            if (header.IdLength != 0)
            {
                var ImageID = stream.ReadBytes(header.IdLength);
                throw new NotImplementedException();
            }

            if (header.ColorMapType != ColorMapType.NONE)
            {
                var ColorMap = stream.ReadBytes(header.ColorMapSpecification.ColormapLength);
                throw new NotImplementedException();
            }

            var specs = header.ImageSpecification;
            if (header.ImageType == ImageType.UNCOMP_TRUECOLOR)
            {
                if (header.ImageSpecification.PixelDepth == 24)
                {
                    var texture = new Bitmap(specs.Width, specs.Height, PixelFormat.Format24bppRgb);
                    var bmpData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly,
                        PixelFormat.Format24bppRgb);
                    var bytes = bmpData.Stride*bmpData.Height;

                    var scanBytes = specs.Width*specs.Height*specs.PixelDepth;
                    var scanlines = stream.ReadBytes(scanBytes);

                    Marshal.Copy(scanlines, 0, bmpData.Scan0, bytes);
                    texture.UnlockBits(bmpData);
                    texture.RotateFlip(RotateFlipType.Rotate180FlipX);
                    return texture;
                }
                
                if (header.ImageSpecification.PixelDepth == 32)
                {
                    var texture = new Bitmap(specs.Width, specs.Height, PixelFormat.Format32bppArgb);
                    var bmpData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);
                    var bytes = bmpData.Stride * bmpData.Height;

                    var scanBytes = specs.Width * specs.Height * specs.PixelDepth;
                    var scanlines = stream.ReadBytes(scanBytes);

                    Marshal.Copy(scanlines, 0, bmpData.Scan0, bytes);
                    texture.UnlockBits(bmpData);
                    texture.RotateFlip(RotateFlipType.Rotate180FlipX);
                    return texture;
                }
            }
            else if (header.ImageType == ImageType.RLE_TRUECOLOR)
            {
                if (specs.PixelDepth == 24)
                {
                    var texture = new Bitmap(specs.Width, specs.Height, PixelFormat.Format24bppRgb);
                    var bmpData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    var bmpSize = bmpData.Stride * bmpData.Height;
                    var bmpPixels = new byte[bmpSize];

                    var tgaSize = specs.Width * specs.Height * specs.PixelDepth;
//                    var tgaPixels = stream.ReadBytes(tgaSize);
//                    var tgaIndex;

                    for (var l = 0; l < bmpData.Height; l++)
                    {
                        var s = l * bmpData.Stride;
                        for (var i = 0; i < bmpData.Width*3;)
                        {
                            byte pkt = stream.ReadByte(); // get first byte of packet
                            int pCnt = (byte) (0x7F & pkt);
                            if (0x80 == (0x80 & pkt)) // if top bit is set, this is a a Run-length Packet
                            {
                                byte pxB = stream.ReadByte();
                                byte pxG = stream.ReadByte();
                                byte pxR = stream.ReadByte();
                                for (var j = 0; j < pCnt + 1; j++) // repeat color for pCnt pixels
                                {
                                    bmpPixels[s + i++] = pxB;
                                    bmpPixels[s + i++] = pxG;
                                    bmpPixels[s + i++] = pxR;
                                }
                            }
                            else // this is a raw packet
                            {
                                for (var j = 0; j < pCnt + 1; j++)
                                {
                                    byte pxB = stream.ReadByte();
                                    byte pxG = stream.ReadByte();
                                    byte pxR = stream.ReadByte();
                                    bmpPixels[s + i++] = pxR;
                                    bmpPixels[s + i++] = pxG;
                                    bmpPixels[s + i++] = pxB;
                                }
                            }
                        }
                    }
                    
                    Marshal.Copy(bmpPixels, 0, bmpData.Scan0, bmpSize);
                    texture.UnlockBits(bmpData);
                    texture.RotateFlip(RotateFlipType.Rotate180FlipX);
                    return texture;
                }
            }

            throw new NotImplementedException();
        }

        [StructLayout(LayoutKind.Sequential, Size = 18, Pack = 1)]
        struct TgaHeader
        {
            public byte IdLength;
            public ColorMapType ColorMapType;
            public ImageType ImageType;
            public ColorMapSpecification ColorMapSpecification;
            public ImageSpecification ImageSpecification;
        }

        [StructLayout(LayoutKind.Explicit, Size = 26, Pack = 1)]
        struct TgaFooter
        {
            private static readonly byte[] TGA_FOOTER_SIG = Encoding.ASCII.GetBytes("TRUEVISION-XFILE");

            [FieldOffset(0)] [MarshalAs(UnmanagedType.U4)]
            public UInt32 ExtensionAreaOffset;

            [FieldOffset(4)] [MarshalAs(UnmanagedType.U4)]
            public UInt32 DeveloperDirectoryOffset;

            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Signature;

            [FieldOffset(24)]
            [MarshalAs(UnmanagedType.U1)]
            public byte Period;

            [FieldOffset(25)]
            [MarshalAs(UnmanagedType.U1)]
            public byte Null;

            public bool NewTga()
            {
                return Signature.Memcmp(TGA_FOOTER_SIG) && Period == 0x2E && Null == 0;
            }
        }

        enum ColorMapType : byte
        {
            NONE = 0,
            INCLUDED = 1
        }

        enum ImageType : byte
        {
            NO_IMAGE = 0,
            UNCOMP_COLORMAPPED = 1,
            UNCOMP_TRUECOLOR = 2,
            UNCOMP_BW = 3,
            RLE_COLORMAPPED = 9,
            RLE_TRUECOLOR = 10,
            RLE_BW = 11
        }

        [StructLayout(LayoutKind.Sequential, Size = 5, Pack = 1)]
        struct ColorMapSpecification
        {
            public readonly UInt16 FirstEntryIndex;
            public readonly UInt16 ColormapLength;
            public readonly Byte ColormapEntrySize;
        }

        [StructLayout(LayoutKind.Sequential, Size = 10, Pack = 1)]
        struct ImageSpecification
        {
            public UInt16 XOrigin;
            public UInt16 YOrigin;
            public UInt16 Width;
            public UInt16 Height;
            public Byte PixelDepth;
            public Byte ImageDescriptor;
        }
    }
}
