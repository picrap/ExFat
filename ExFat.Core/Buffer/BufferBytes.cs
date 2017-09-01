namespace ExFat.Core.Buffer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents bytes in the buffer
    /// </summary>
    /// <seealso cref="BufferData" />
    [DebuggerDisplay("{" + nameof(DebugLiteral) + "}")]
    public class BufferBytes : BufferData, IEnumerable<byte>
    {
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
            get { return GetAt(index); }
            set { SetAt(index, value); }
        }

        private string DebugLiteral
        {
            get
            {
                var bytes = GetAll();
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
        public BufferBytes(byte[] buffer, int offset, int length)
            : base(buffer, offset, length)
        {
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)GetAll()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
