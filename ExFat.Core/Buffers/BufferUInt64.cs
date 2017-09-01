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
            get { return BitConverter.ToUInt64(_buffer.GetBytes().FromLittleEndian(), 0); }
            set { _buffer.Set(BitConverter.GetBytes(value).ToLittleEndian()); }
        }

        public BufferUInt64(Buffers.Buffer buffer, int offset)
        {
            _buffer = new Buffers.Buffer(buffer, offset, sizeof(UInt64));
        }
    }
}
