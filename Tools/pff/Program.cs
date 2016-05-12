using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Novalogic.Archive;

namespace Pff
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            FileInfo inputFile = null;
            var extract = false;
            var list = false;

            var i = 0;
            while (i < args.Length)
            {
                var arg = args[i++];
                switch (arg)
                {
                    case "-l":
                        list = true;
                        break;

                    case "-e":
                        extract = true;
                        break;

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
                if (list)
                {
                    foreach (var pffEntry in archive.Entries)
                    {
                        Console.WriteLine(pffEntry.FilePath);
                    }
                }

                if (extract)
                {
                    if (!CanWriteToFolder(inputFile.DirectoryName))
                        throw new Exception("Cannot write to archive folder.");

                    var archiveName = Path.GetFileNameWithoutExtension(inputFile.Name);
                    var extractDir = !string.IsNullOrWhiteSpace(archiveName) ? archiveName : "EXTRACTED";
                    Directory.CreateDirectory(extractDir);

                    foreach (var pffEntry in archive.Entries)
                    {
                        var filePath = Path.Combine(extractDir, pffEntry.FilePath);
                        var contents = pffEntry.GetContents();

                        if (contents != null)
                        {
                            File.WriteAllBytes(filePath, contents);
                            File.SetLastWriteTime(filePath, pffEntry.PackedTimeUtc.ToLocalTime());
                        }
                    }
                }
            }
        }

        private static bool CanWriteToFolder(string folder)
        {
            var permission = new FileIOPermission(FileIOPermissionAccess.Write, folder);
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(permission);
            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }
    }
}