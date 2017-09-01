namespace ExFat.Core.Buffers
{
    using System;
    using System.Collections.Generic;

    public class Buffer
    {
        private readonly int _offset;
        private readonly byte[] _bytes;

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
                return _bytes[_offset + index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _bytes[_offset + index] = value;
            }
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            var bytes = new byte[Length];
            Array.Copy(_bytes, _offset, bytes, 0, Length);
            return bytes;
        }

        public void Set(IList<byte> bytes)
        {
            if (bytes.Count > Length)
                throw new ArgumentException(nameof(bytes));
            for (int index = 0; index < bytes.Count; index++)
                _bytes[_offset + index] = bytes[index];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class given a raw array of bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public Buffer(byte[] bytes)
        {
            _bytes = bytes;
            Length = _bytes.Length;
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
            _bytes = buffer._bytes;
            _offset = buffer._offset + offset;
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
