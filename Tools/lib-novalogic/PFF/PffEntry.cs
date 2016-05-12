using System;
using System.IO;

namespace Novalogic.PFF
{
    public class PffEntry
    {
        private readonly PffArchive.IPffEntry _entry;
        private readonly BinaryReader _reader;

        public PffEntry(BinaryReader reader, PffArchive.IPffEntry entry)
        {
            _reader = reader;
            _entry = entry;
        }

        public uint FileSize => _entry.FileSize;
        public DateTime PackedTimeUtc => new DateTime(1970, 1, 1).AddSeconds(_entry.FileModified);
        public string FilePath => Utility.TextEncode.GetString(_entry.FileName, 0, _entry.FileName.Length).TrimEnd('\0');

        /// <summary>
        ///     Retrieves the contents of the file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="ArgumentException">
        ///     The number of decoded characters to read is greater than FileSize. This can happen
        ///     if a Unicode decoder returns fallback characters or a surrogate pair.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     FileSize is negative.
        /// </exception>
        public byte[] GetContents()
        {
            if (_entry.Deleted != 0 || _entry.FileOffset == uint.MaxValue)
                return null;

            var stream = _reader.BaseStream;
            var oldFilePos = stream.Position;
            stream.Position = _entry.FileOffset;
            var file = _reader.ReadBytes((int) _entry.FileSize);
            stream.Position = oldFilePos;
            return file;
        }
    }
}