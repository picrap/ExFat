// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat
{
    using System;
    using System.Collections.Generic;
    using Buffer = Buffers.Buffer;

    /// <summary>
    /// Methods to convert between bytes and integers
    /// </summary>
    public static class LittleEndian
    {
        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="result">The result.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static void GetBytes(UInt64 v, byte[] result, int offset = 0)
        {
            result[offset] = (byte) (v & 0xFF);
            result[offset + 1] = (byte) ((v >> 8) & 0xFF);
            result[offset + 2] = (byte) ((v >> 16) & 0xFF);
            result[offset + 3] = (byte) ((v >> 24) & 0xFF);
            result[offset + 4] = (byte) ((v >> 32) & 0xFF);
            result[offset + 5] = (byte) ((v >> 40) & 0xFF);
            result[offset + 6] = (byte) ((v >> 48) & 0xFF);
            result[offset + 7] = (byte) ((v >> 56) & 0xFF);
        }

        /// <summary>
        /// Gets the bytes from given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="buffer">The buffer.</param>
        public static void GetBytes(UInt64 v, Buffer buffer)
        {
            GetBytes(v, buffer.Bytes, buffer.Offset);
        }

        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt64 v)
        {
            var result = new byte[8];
            GetBytes(v, result);
            return result;
        }

        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="result">The result.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static void GetBytes(UInt32 v, byte[] result, int offset = 0)
        {
            result[offset] = (byte) (v & 0xFF);
            result[offset + 1] = (byte) ((v >> 8) & 0xFF);
            result[offset + 2] = (byte) ((v >> 16) & 0xFF);
            result[offset + 3] = (byte) ((v >> 24) & 0xFF);
        }


        /// <summary>
        /// Gets the bytes from given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="buffer">The buffer.</param>
        public static void GetBytes(UInt32 v, Buffer buffer)
        {
            GetBytes(v, buffer.Bytes, buffer.Offset);
        }

        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt32 v)
        {
            var result = new byte[4];
            GetBytes(v, result);
            return result;
        }

        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="result">The result.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static void GetBytes(UInt16 v, byte[] result, int offset = 0)
        {
            result[offset] = (byte) (v & 0xFF);
            result[offset + 1] = (byte) ((v >> 8) & 0xFF);
        }

        /// <summary>
        /// Gets the bytes from given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <param name="buffer">The buffer.</param>
        public static void GetBytes(UInt16 v, Buffer buffer)
        {
            GetBytes(v, buffer.Bytes, buffer.Offset);
        }


        /// <summary>
        /// Gets the bytes from the given value.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt16 v)
        {
            var result = new byte[2];
            GetBytes(v, result);
            return result;
        }

        /// <summary>
        /// Extracts an <see cref="UInt64"/> from bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt64 ToUInt64(IList<byte> bytes, int offset = 0)
        {
            return bytes[offset]
                   | (UInt64) bytes[offset + 1] << 8
                   | (UInt64) bytes[offset + 2] << 16
                   | (UInt64) bytes[offset + 3] << 24
                   | (UInt64) bytes[offset + 4] << 32
                   | (UInt64) bytes[offset + 5] << 40
                   | (UInt64) bytes[offset + 6] << 48
                   | (UInt64) bytes[offset + 7] << 56;
        }

        /// <summary>
        /// Extracts an <see cref="UInt64" /> from bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static UInt64 ToUInt64(Buffer buffer)
        {
            return ToUInt64(buffer.Bytes, buffer.Offset);
        }

        /// <summary>
        /// Extracts an <see cref="UInt32"/> from bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt32 ToUInt32(IList<byte> bytes, int offset = 0)
        {
            return bytes[offset]
                   | (UInt32) bytes[offset + 1] << 8
                   | (UInt32) bytes[offset + 2] << 16
                   | (UInt32) bytes[offset + 3] << 24;
        }


        /// <summary>
        /// Extracts an <see cref="UInt32" /> from bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static UInt32 ToUInt32(Buffer buffer)
        {
            return ToUInt32(buffer.Bytes, buffer.Offset);
        }

        /// <summary>
        /// Extracts an <see cref="UInt32"/> from bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt16 ToUInt16(IList<byte> bytes, int offset = 0)
        {
            return (UInt16) (bytes[offset]
                             | bytes[offset + 1] << 8);
        }

        /// <summary>
        /// Extracts an <see cref="UInt16" /> from bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static UInt16 ToUInt16(Buffer buffer)
        {
            return ToUInt16(buffer.Bytes, buffer.Offset);
        }
    }
}