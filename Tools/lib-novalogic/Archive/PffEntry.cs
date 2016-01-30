using System;
using System.IO;
using System.Text;

namespace Novalogic.Archive
{
    public class PffEntry
    {
        private readonly BinaryReader _br;

        public PffEntry(BinaryReader reader)
        {
            _br = reader;

            Deleted = reader.ReadInt32() != 0;
            FilePosition = reader.ReadInt32();
            FileSize = reader.ReadInt32();

            var timeT = reader.ReadUInt32();
            PackedTimeUTC = new DateTime(1970, 1, 1).AddSeconds(timeT);

            var bName = reader.ReadBytes(0x0F);
            FilePath = Encoding.ASCII.GetString(bName, 0, bName.Length);
            FilePath = FilePath.TrimEnd('\0');

            //NOTE: There may be more fields after this, they are ignored
        }

        public bool Deleted { get; }
        public int FilePosition { get; }
        public int FileSize { get; }
        public DateTime PackedTimeUTC { get; private set; }
        public string FilePath { get; }

        public byte[] GetFile()
        {
            if (Deleted || FilePosition == -1)
                return null;

            var oldFilePos = _br.BaseStream.Position;
            _br.BaseStream.Position = FilePosition;

            var file = _br.ReadBytes(FileSize);

            _br.BaseStream.Position = oldFilePos;
            return file;
        }
    }
}