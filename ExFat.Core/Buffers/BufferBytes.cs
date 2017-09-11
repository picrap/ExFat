// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents bytes in the buffer
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugLiteral) + "}")]
    public class BufferBytes : IEnumerable<byte>
    {
        private readonly Buffer _buffer;

        /// <summary>
        /// Gets or sets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Byte"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public byte this[int index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }

        private string DebugLiteral
        {
            get
            {
                var bytes = _buffer.GetBytes();
                var s = string.Join(", ", bytes.Take(10).Select(b => $"0x{b:X2}"));
                if (bytes.Length > 10)
                    s += " ...";
                return s;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferBytes"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public BufferBytes(Buffer buffer, int offset, int length)
        {
            _buffer = new Buffer(buffer, offset, length);
        }

        /// <summary>
        /// Sets the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public void Set(IList<byte> bytes)
        {
            for (var offset = 0; offset < _buffer.Length; offset++)
                _buffer[offset] = bytes[offset];
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>) _buffer.GetBytes()).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}