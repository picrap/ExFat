// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// 8-bit unsigned int buffer (in other words, a byte)
    /// </summary>
    /// <seealso cref="byte" />
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt8 : IValueProvider<Byte>
    {
        private readonly Buffer _buffer;

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public Byte Value
        {
            get { return _buffer[0]; }
            set { _buffer[0] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferUInt8"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        public BufferUInt8(Buffer buffer, int offset)
        {
            _buffer = new Buffer(buffer, offset, 1);
        }
    }
}