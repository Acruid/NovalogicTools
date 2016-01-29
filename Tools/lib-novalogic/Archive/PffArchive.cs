using System;
using System.Collections.Generic;
using System.IO;

namespace Novalogic.Archive
{
    public class PffArchive : IDisposable
    {
        private readonly BinaryReader _bReader;
        private readonly Pff3Header _header;

        private PffArchive(BinaryReader reader)
        {
            _bReader = reader;
            _header = new Pff3Header(reader);
        }

        public IEnumerable<PffEntry> Entries => GenerateFileEntries();

        public void Dispose()
        {
            _bReader.Dispose();
        }

        public static PffArchive Open(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return null;

            var reader = new BinaryReader(fileInfo.OpenRead());

            return new PffArchive(reader);
        }

        private IEnumerable<PffEntry> GenerateFileEntries()
        {
            var entries = new List<PffEntry>();

            for (var i = 0; i < _header.RecordCount; i++)
            {
                _bReader.BaseStream.Position = _header.FirstRecordPosition + _header.RecordLength*i;

                var entry = new PffEntry(_bReader);
                entries.Add(entry);
            }

            return entries;
        }
    }
}