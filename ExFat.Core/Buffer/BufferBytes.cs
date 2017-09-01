namespace ExFat.Core.Buffer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents bytes in the buffer
    /// </summary>
    /// <seealso cref="ExFat.Core.BufferData" />
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
