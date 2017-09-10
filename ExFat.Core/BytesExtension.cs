// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat
{
    using System;

    /// <summary>
    /// Extenstions to <see cref="byte"/>[]
    /// </summary>
    public static class BytesExtension
    {
        /// <summary>
        /// Gets the checksum of the given buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="checksum">The checksum.</param>
        /// <returns></returns>
        public static UInt16 GetChecksum16(this byte[] bytes, int offset, int count, UInt16 checksum = 0)
        {
            count += offset;
            for (int index = offset; index < count; index++)
                checksum = (UInt16)(checksum.RotateRight() + bytes[index]);
            return checksum;
        }

        /// <summary>
        /// Gets the checksum of the given buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="checksum">The checksum.</param>
        /// <returns></returns>
        public static UInt32 GetChecksum32(this byte[] bytes, int offset, int count, UInt32 checksum = 0)
        {
            count += offset;
            for (int index = offset; index < count; index++)
                checksum = checksum.RotateRight() + bytes[index];
            return checksum;
        }
    }
}
