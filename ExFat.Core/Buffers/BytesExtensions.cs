namespace ExFat.Core.Buffers
{
    using System;
    using System.Linq;

    public static class BytesExtensions
    {
        /// <summary>
        /// Converts a byte array to little endian.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static byte[] ToLittleEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                return bytes;
            // slow, but unused
            return bytes.Reverse().ToArray();
        }

        /// <summary>
        /// Froms a byte array from little endian.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static byte[] FromLittleEndian(this byte[] bytes) => ToLittleEndian(bytes);
    }
}
