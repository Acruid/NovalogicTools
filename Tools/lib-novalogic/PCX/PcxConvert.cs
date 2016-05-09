using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Novalogic.PCX
{
    // References:
    // http://bespin.org/~qz/pc-gpe/pcx.txt

    /// <summary>
    ///     Class containing functions for importing/exporting PCX files.
    /// </summary>
    public static class PcxConvert
    {
        private const int PALETTE_MARKER = 0x0C;

        /// <summary>
        ///     Converts a pcx image into a Bitmap image.
        /// </summary>
        /// <param name="fileBytes">Bytes of the PCX file.</param>
        /// <returns>Bitmap of the PCX file.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="Exception">The operation failed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        public static Bitmap LoadPcx(byte[] fileBytes)
        {
            if (fileBytes.Length < 0x80)
                return null;

            using (var stream = new BinaryReader(new MemoryStream(fileBytes)))
            {
                return LoadPcx(stream);
            }
        }

        /// <summary>
        ///     Converts a pcx image into a Bitmap image.
        /// </summary>
        /// <param name="stream">Stream of the PCX file.</param>
        /// <returns>Bitmap of the PCX file.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="Exception">The operation failed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        private static Bitmap LoadPcx(BinaryReader stream)
        {
            // Reading Header
            var headerBytes = stream.ReadBytes(128);
            var hdr = headerBytes.ToStruct<PcxHeader>();

            var rgbPlanes = hdr.Version >= PcxVersion.VERSION_3_0 && hdr.BitsPerPixel == 8 && hdr.NPlanes == 3;
            var appendedPalette = !rgbPlanes && hdr.Version >= PcxVersion.VERSION_3_0;

            var xSize = hdr.Window.Xmax - hdr.Window.Xmin + 1;
            var ySize = hdr.Window.Ymax - hdr.Window.Ymin + 1;
            //var totalLineBytes = hdr.NPlanes*hdr.BytesPerLine;

            // Reading Scanlines
            var lsize = (long) hdr.BytesPerLine*hdr.NPlanes*(1 + hdr.Window.Ymax - hdr.Window.Ymin);
            var pxNum = 0;
            var pixels = new byte[lsize];
            for (var l = 0; l < lsize;) // increment by cnt below
            {
                byte chr, cnt;
                Encget(out chr, out cnt, stream);
                int i;
                //TODO: There should always be a decoding break at the end of each scan line.
                for (i = 0; i < cnt; i++)
                    pixels[pxNum++] = chr;
                l += cnt;
            }

            byte[] palette;

            //Reading optional Palette
            if (appendedPalette)
            {
                var paletteMarker = stream.ReadByte();
                if (paletteMarker != PALETTE_MARKER)
                    throw new Exception("PCX image should have an appended 256 color palette, but marker != 12.");

                palette = stream.ReadBytes(3*256);
            }
            else
            {
                palette = hdr.Colormap;
            }

            // build the 24bit RGB bitmap
            if (rgbPlanes)
                throw new NotImplementedException("24bit RGB planes not supported.");

            var texture = new Bitmap(xSize, ySize, PixelFormat.Format24bppRgb);
            var bmpData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var bytes = bmpData.Stride*bmpData.Height;
            var texBuffer = new byte[bytes];

            //turn indexed pixels into 24bit rbg pixels
            for (var i = 0; i < lsize; i++)
            {
                var index = pixels[i]*3;
                texBuffer[i*3 + 2] = palette[index + 0]; // R
                texBuffer[i*3 + 1] = palette[index + 1]; // G
                texBuffer[i*3 + 0] = palette[index + 2]; // B
            }

            Marshal.Copy(texBuffer, 0, bmpData.Scan0, bytes);
            texture.UnlockBits(bmpData);
            return texture;
        }

        /// <summary>
        ///     This procedure reads one encoded block from the image file and stores a count and data byte.
        /// </summary>
        /// <param name="pbyt">where to place data</param>
        /// <param name="pcnt">where to place count</param>
        /// <param name="fid">image file handle</param>
        /// <returns> 0 = valid data stored, EOF = out of data in file</returns>
        private static void Encget(out byte pbyt, out byte pcnt, BinaryReader fid)
        {
            pcnt = 1; // assume a "run" length of one
            var i = fid.ReadByte();
            if (0xC0 == (0xC0 & i)) // if top two bits are set
            {
                pcnt = (byte) (0x3F & i); // remaining six bits are how many times to dupe next byte
                i = fid.ReadByte(); // next byte to dupe
            }
            pbyt = i;
        }

        [StructLayout(LayoutKind.Explicit, Size = 128, Pack = 1)]
        private struct PcxHeader
        {
            [FieldOffset(0)] [MarshalAs(UnmanagedType.U1)] public readonly PcxManufacturer Manufacturer;

            [FieldOffset(1)] [MarshalAs(UnmanagedType.U1)] public readonly PcxVersion Version;

            [FieldOffset(2)] [MarshalAs(UnmanagedType.U1)] public readonly PcxEncoding Encoding;

            [FieldOffset(3)] [MarshalAs(UnmanagedType.U1)] public readonly byte BitsPerPixel;

            [FieldOffset(4)] [MarshalAs(UnmanagedType.Struct)] public PcxWindow Window;

            [FieldOffset(12)] [MarshalAs(UnmanagedType.U2)] public readonly ushort HDpi;

            [FieldOffset(14)] [MarshalAs(UnmanagedType.U2)] public readonly ushort VDpi;

            [FieldOffset(16)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)] public readonly byte[] Colormap;

            [FieldOffset(64)] [MarshalAs(UnmanagedType.U1)] public readonly byte Reserved;

            [FieldOffset(65)] [MarshalAs(UnmanagedType.U1)] public readonly byte NPlanes;

            [FieldOffset(66)] [MarshalAs(UnmanagedType.U2)] public readonly ushort BytesPerLine;

            [FieldOffset(68)] [MarshalAs(UnmanagedType.U2)] public readonly PcxPaletteType PaletteInfo;

            [FieldOffset(70)] [MarshalAs(UnmanagedType.U2)] public readonly ushort HscreenSize;

            [FieldOffset(72)] [MarshalAs(UnmanagedType.U2)] public readonly ushort VscreenSize;

/*
            // This needs to be 4-byte aligned, but is pointless anyways since the
            // array should always contain 0x00.
            [FieldOffset(74)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 54)] public Byte[] Filler;
*/
        }

        private enum PcxManufacturer : byte
        {
            ZSOFT = 10
        }

        private enum PcxVersion : byte
        {
            VERSION_2_5 = 0,
            VERSION_2_8_PALETTE = 2,
            VERSION_2_8_DEFAULT_PALETTE = 3,
            VERSION_3_0 = 5
        }

        private enum PcxEncoding : byte
        {
            NONE = 0,
            RUN_LENGTH = 1
        }

        private enum PcxPaletteType : ushort
        {
            INDEXED = 1,
            GRAYSCALE = 2
        }

        [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
        private struct PcxWindow
        {
            [FieldOffset(0)] [MarshalAs(UnmanagedType.U2)] public readonly ushort Xmin;

            [FieldOffset(2)] [MarshalAs(UnmanagedType.U2)] public readonly ushort Ymin;

            [FieldOffset(4)] [MarshalAs(UnmanagedType.U2)] public readonly ushort Xmax;

            [FieldOffset(6)] [MarshalAs(UnmanagedType.U2)] public readonly ushort Ymax;
        }
    }
}