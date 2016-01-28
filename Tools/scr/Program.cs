using System;
using System.Collections.Generic;
using System.IO;

namespace SCR
{
    internal static class Program
    {
        private const uint SCR_HEADER = 0x01524353;
        private const int SCR_HEADER_SZ = 0x04;

        private static Dictionary<string, uint> _keys;

        private static void Main(string[] args)
        {
            _keys = new Dictionary<string, uint>
            {
                {"DF1", 0x04960552},
                {"DF2", 0x01234567},
                {"DF3", 0x01234567},
                {"TFD", 0x01234567},
                {"C4", 0x01234567},
                {"BHD", 0x2A5A8EAD},
                {"JO", 0x2A5A8EAD},
                {"TS", 0x2A5A8EAD},
                {"JO:B", 0xABEEFACE}
            };

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

                            if (_keys.ContainsKey(keyStr))
                            {
                                key = _keys[keyStr];
                            }
                            else if (keyStr.Length == 10)
                            {
                                key = Convert.ToUInt32(keyStr.Substring(2), 16);
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

            ConvertFile(inputFile, mode, key);
        }

        private static void ConvertFile(FileInfo file, Mode mode, uint key)
        {
            using (var stream = file.OpenRead())
            {
                using (var reader = new BinaryReader(stream))
                {
                    if (mode == Mode.DECODE)
                    {
                        if (stream.Length < SCR_HEADER_SZ)
                            throw new IOException("Set to decode, but input header is corrupt.");

                        var header = reader.ReadUInt32();
                        if (header != SCR_HEADER)
                            throw new Exception("Set to decode, input header is wrong.");

                        var body = reader.ReadBytes((int) (stream.Length - stream.Position));

                        SwapByteOrder(body);

                        XorBlock(body, key, body.Length);

                        File.WriteAllBytes(file.FullName + ".txt", body);
                    }
                    else if (mode == Mode.ENCODE)
                    {
                        var body = reader.ReadBytes((int) stream.Length);

                        XorBlock(body, key, body.Length);

                        SwapByteOrder(body);

                        var newName = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length);

                        using (var output = File.OpenWrite(newName))
                        {
                            using (var writer = new BinaryWriter(output))
                            {
                                writer.Write(SCR_HEADER);
                                writer.Write(body);
                            }
                        }
                    }
                }
            }
        }

        private static void SwapByteOrder(byte[] input)
        {
            var len = input.Length;
            var half = len/2;

            if (half <= 0)
                return;

            var front = 0;
            var back = len - 1;
            var i = half;
            do
            {
                var temp = input[front];
                input[front] = input[back];
                input[back] = temp;

                front++;
                back--;
                i--;
            } while (i > 0);
        }

        private static void XorBlock(byte[] input, uint key, int length)
        {
            var i = 0;

            if (length <= 0)
                return;

            do
            {
                var temp1 = key.RotateLeft(11);
                var temp2 = (key + temp1).RotateLeft(4);

                key = temp2 ^ 1;

                input[i] ^= (byte) (temp2 ^ 1);

                i++;
            } while (i < length);
        }

        private static uint RotateLeft(this uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        private enum Mode
        {
            ERROR = 0,
            DECODE = 1,
            ENCODE = 2
        }
    }
}