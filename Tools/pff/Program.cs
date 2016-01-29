using System;
using System.IO;
using Novalogic.Archive;

namespace Pff
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            FileInfo inputFile = null;

            var i = 0;
            while (i < args.Length)
            {
                var arg = args[i++];
                switch (arg)
                {
                    default:
                        if (File.Exists(arg))
                            inputFile = new FileInfo(arg);
                        else
                            throw new ArgumentException($"Program args, unknown/missing file: {arg}");
                        break;
                }
            }

            if (inputFile == null)
                throw new ArgumentException("No input file was specified.");

            using (var archive = PffArchive.Open(inputFile))
            {
                foreach (var pffEntry in archive.Entries)
                {
                    Console.WriteLine(pffEntry.FilePath);
                }
            }
        }
    }
}