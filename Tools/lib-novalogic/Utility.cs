using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Novalogic
{
    /// <summary>
    /// Utility functions used throughout the library.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Text encoding of nl files.
        /// </summary>
        public static readonly Encoding TextEncode = Encoding.ASCII;

        /// <summary>
        /// Converts a byte array into a struct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] bytes)
        {
            if (bytes == null)
                return default(T);

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T obj;
            try
            {
                var ptr = handle.AddrOfPinnedObject();
                obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return obj;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        /// <summary>
        /// Compares two byte arrays for equality.
        /// </summary>
        /// <param name="b1">Array one.</param>
        /// <param name="b2">Array two.</param>
        /// <returns>Equality of the two arrays.</returns>
        /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
        public static bool Memcmp(this byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
    }
}
