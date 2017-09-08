// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a byte buffer. May be a part of a bigger buffer.
    /// </summary>
    public class Buffer
    {
        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        public byte[] Bytes { get; }

        /// <summary>
        /// Gets the start offset in bytes.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; }

        /// <summary>
        /// Gets or sets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Byte"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index
        /// or
        /// index
        /// </exception>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Bytes[Offset + index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                Bytes[Offset + index] = value;
            }
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            var bytes = new byte[Length];
            Array.Copy(Bytes, Offset, bytes, 0, Length);
            return bytes;
        }

        /// <summary>
        /// Sets the specified bytes to the buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <exception cref="System.ArgumentException">bytes</exception>
        public void Set(IList<byte> bytes)
        {
            if (bytes.Count > Length)
                throw new ArgumentException(nameof(bytes));
            for (int index = 0; index < bytes.Count; index++)
                Bytes[Offset + index] = bytes[index];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class given a raw array of bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public Buffer(byte[] bytes)
        {
            Bytes = bytes;
            Length = Bytes.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class given another <see cref="Buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset
        /// or
        /// length
        /// </exception>
        public Buffer(Buffer buffer, int offset, int length)
        {
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            Bytes = buffer.Bytes;
            Offset = buffer.Offset + offset;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class given another <see cref="Buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public Buffer(Buffer buffer)
            : this(buffer, 0, buffer.Length)
        {
        }
    }
}