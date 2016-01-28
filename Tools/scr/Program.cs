using System;
using System.IO;
using Novalogic.Script;

namespace SCR
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            FileInfo inputFile = null;
            var mode = Mode.ERROR;
            uint key = 0x0;

            var i = 0;
            while (i < args.Length)
            {
                var arg = args[i++];
                switch (arg)
                {
                    case "-d":
                        mode = Mode.DECODE;
                        break;

                    case "-e":
                        mode = Mode.ENCODE;
                        break;

                    case "-k":
                        if (i + 1 <= args.Length)
                        {
                            var keyStr = args[i++];

                            if (!ScriptFile.TryGetKey(keyStr, out key))
                            {
                                if (keyStr.Length == 10)
                                {
                                    key = Convert.ToUInt32(keyStr.Substring(2), 16);
                                }
                            }
                        }
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
            if (mode == Mode.ERROR)
                throw new ArgumentException("No mode was specified.");
            if (key == 0x0)
                throw new ArgumentException("No key was specified.");

            using (var stream = inputFile.OpenRead())
            {
                using (var reader = new BinaryReader(stream))
                {
                    var body = reader.ReadBytes((int) stream.Length);

                    if (mode == Mode.DECODE)
                    {
                        body = ScriptFile.Decrypt(body, key);

                        File.WriteAllBytes(inputFile.FullName + ".txt", body);
                    }
                    else if (mode == Mode.ENCODE)
                    {
                        body = ScriptFile.Encrypt(body, key);

                        var newName = inputFile.FullName.Substring(0,
                            inputFile.FullName.Length - inputFile.Extension.Length);

                        File.WriteAllBytes(newName, body);
                    }
                }
            }
        }

        private enum Mode
        {
            ERROR = 0,
            DECODE = 1,
            ENCODE = 2
        }
    }
}