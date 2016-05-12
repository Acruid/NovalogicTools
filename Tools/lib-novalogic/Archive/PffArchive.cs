using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Novalogic.Archive
{
    /// <summary>
    /// </summary>
    public class PffArchive : IDisposable
    {
        private readonly BinaryReader _bReader;
        private readonly Header_Pff3_20 _header;
        private List<PffEntry> _cachedEntries;

        private PffArchive(FileInfo fileInfo)
        {
            var reader = new BinaryReader(fileInfo.OpenRead());
            FileInfo = fileInfo;
            _bReader = reader;

            var headerSize = reader.ReadUInt32();
            var version = (PffVersion) reader.ReadUInt32();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (version == PffVersion.PFF3 && headerSize == 20)
                _header = reader.ReadBytes(20).ToStruct<Header_Pff3_20>();
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// </summary>
        public IEnumerable<PffEntry> Entries => EnumerateDirectory();

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            _bReader.Dispose();
        }

        /// <summary>
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static PffArchive Open(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return null;

            return new PffArchive(fileInfo);
        }

        /// <summary>
        ///     Gets the file entry of the named file.
        /// </summary>
        /// <param name="fileName">Name of file to get.</param>
        /// <returns>Fileentry of filename.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Source is null.
        /// </exception>
        public PffEntry GetEntry(string fileName)
        {
            return EnumerateDirectory().FirstOrDefault(fileEntry => fileEntry.FilePath == fileName);
        }

        private IEnumerable<PffEntry> EnumerateDirectory()
        {
            if (_cachedEntries != null)
                return _cachedEntries;

            var entries = new List<PffEntry>();
            _bReader.BaseStream.Position = _header.RecordOffset + _header.RecordSize;

            for (var i = 0; i < _header.RecordCount; i++)
            {
                PffEntry pffEntry;
                if (_header.Signature == PffVersion.PFF3)
                {
                    if (_header.RecordSize == 32)
                    {
                        var entry = _bReader.ReadBytes(32).ToStruct<Entry_Pff3_32>();
                        pffEntry = new PffEntry(_bReader, entry);
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                    throw new NotImplementedException();

                entries.Add(pffEntry);
            }

            _cachedEntries = entries;
            return entries;
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IPffEntry
        {
            UInt32 Deleted { get; }
            UInt32 FileOffset { get; }
            UInt32 FileSize { get; }
            UInt32 FileModified { get; }
            Byte[] FileName { get; }
        }

        // ReSharper disable UnassignedGetOnlyAutoProperty

        [StructLayout(LayoutKind.Sequential, Size = 20, Pack = 1)]
        private struct Header_Pff3_20
        {
            public UInt32 HeaderSize { get; }
            public PffVersion Signature { get; }
            public UInt32 RecordCount { get; }
            public UInt32 RecordSize { get; }
            public UInt32 RecordOffset { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = 32, Pack = 1)]
        private struct Entry_Pff3_32 : IPffEntry
        {
            public UInt32 Deleted { get; }
            public UInt32 FileOffset { get; }
            public UInt32 FileSize { get; }
            public UInt32 FileModified { get; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] private readonly Byte[] fileName;
            public Byte[] FileName => fileName;
            public Byte Null { get; }
        }

        private enum PffVersion : uint
        {
            PFF3 = 0x33464650 //{'P','F','F','3'}
        }

        // ReSharper restore UnassignedGetOnlyAutoProperty
    }
}