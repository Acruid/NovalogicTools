using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Novalogic.Archive
{
    public class PffArchive : IDisposable
    {
        private readonly BinaryReader _bReader;
        private readonly Pff3Header _header;
        private List<PffEntry> _cachedEntries;

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

        /// <summary>
        /// Gets the file entry of the named file.
        /// </summary>
        /// <param name="fileName">Name of file to get.</param>
        /// <returns>Fileentry of filename.</returns>
        public PffEntry GetEntry(string fileName)
        {
            return GenerateFileEntries().FirstOrDefault(fileEntry => fileEntry.FilePath == fileName);
        }

        private IEnumerable<PffEntry> GenerateFileEntries()
        {
            if (_cachedEntries != null)
                return _cachedEntries;

            var entries = new List<PffEntry>();

            for (var i = 0; i < _header.RecordCount; i++)
            {
                _bReader.BaseStream.Position = _header.FirstRecordPosition + _header.RecordLength*i;

                var entry = new PffEntry(_bReader);
                entries.Add(entry);
            }

            _cachedEntries = entries;
            return entries;
        }
    }
}