// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// 64-bits unsigned int buffer
    /// </summary>
    /// <seealso cref="ulong" />
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt64 : IValueProvider<UInt64>
    {
        private readonly Buffer _buffer;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public UInt64 Value
        {
            get { return LittleEndian.ToUInt64(_buffer); }
            set { LittleEndian.GetBytes(value, _buffer); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferUInt64"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        public BufferUInt64(Buffer buffer, int offset)
        {
            _buffer = new Buffer(buffer, offset, sizeof(UInt64));
        }
    }
}