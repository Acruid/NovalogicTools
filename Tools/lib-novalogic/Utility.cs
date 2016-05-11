using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Novalogic
{
    /// <summary>
    ///     Utility functions used throughout the library.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        ///     Text encoding of nl files.
        /// </summary>
        public static readonly Encoding TextEncode = Encoding.ASCII;

        /// <summary>
        ///     Converts a byte array into a formatted object.
        ///     The object must be decorated with <see cref="StructLayoutAttribute" /> set to <see cref="LayoutKind.Explicit" /> or
        ///     <see cref="LayoutKind.Sequential" />.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="bytes">The byte array of the unmanaged object to convert.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">An instance with nonprimitive (non-blittable) members cannot be pinned. </exception>
        /// <exception cref="InvalidOperationException">The handle is any type other than <see cref="F:System.Runtime.InteropServices.GCHandleType.Pinned" />. </exception>
        /// <exception cref="ArgumentNullException">
        ///         <typeparam name="T" /> is null.</exception>
        /// <exception cref="MissingMethodException">The class specified by <typeparam name="T" /> does not have an accessible default constructor. </exception>
        public static T ToStruct<T>(this byte[] bytes)
        {
            if (bytes == null)
                return default(T);

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T obj;
            try
            {
                var ptr = handle.AddrOfPinnedObject();
                obj = (T) Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return obj;
        }

        /// <summary>
        ///     Converts a formatted object into a byte array.
        ///     The object must be decorated with <see cref="StructLayoutAttribute" /> set to <see cref="LayoutKind.Explicit" /> or
        ///     <see cref="LayoutKind.Sequential" />.
        /// </summary>
        /// <param name="structure">The object to convert.</param>
        /// <returns>Unmanaged object bytes.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="structure" /> parameter is null.</exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
        /// <exception cref="ArgumentException">
        ///         <paramref name="structure" /> is a reference type that is not a formatted class. -or-<paramref name="structure" /> is a generic type. </exception>
        public static byte[] ToBytes(object structure)
        {
            var size = Marshal.SizeOf(structure);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        /// <summary>
        ///     Compares two byte arrays for equality.
        /// </summary>
        /// <param name="b1">Array one.</param>
        /// <param name="b2">Array two.</param>
        /// <returns>Equality of the two arrays.</returns>
        /// <exception cref="OverflowException">
        ///     The array is multidimensional and contains more than
        ///     <see cref="F:System.Int32.MaxValue" /> elements.
        /// </exception>
        public static bool Memcmp(this byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
    }
}