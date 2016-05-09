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
    }
}
