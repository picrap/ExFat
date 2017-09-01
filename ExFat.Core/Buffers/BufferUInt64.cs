namespace ExFat.Core.Buffers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt64 : IValueProvider<UInt64>
    {
        private readonly Buffers.Buffer _buffer;

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

        public BufferUInt64(Buffers.Buffer buffer, int offset)
        {
            _buffer = new Buffers.Buffer(buffer, offset, sizeof(UInt64));
        }
    }
}
