using System;
using System.IO;

namespace Novalogic.Archive
{
    internal class Pff3Header
    {
        public Pff3Header(BinaryReader reader)
        {
            reader.BaseStream.Position += 4;

            if (reader.ReadUInt32() != 0x33464650) //{'P','F','F','3'}
                throw new Exception("File is not a PFF3.");

            RecordCount = reader.ReadInt32();
            RecordLength = reader.ReadInt32();
            FirstRecordPosition = reader.ReadInt32();
        }

        public int RecordCount { get; private set; }
        public int FirstRecordPosition { get; private set; }
        public int RecordLength { get; private set; }
    }
}