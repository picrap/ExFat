namespace ExFat.Core.Buffers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt32 : IValueProvider<UInt32>
    {
        private readonly Buffer _buffer;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public UInt32 Value
        {
            get { return BitConverter.ToUInt32(_buffer.GetBytes().FromLittleEndian(), 0); }
            set { _buffer.Set(BitConverter.GetBytes(value).ToLittleEndian()); }
        }

        public BufferUInt32(Buffers.Buffer buffer, int offset)
        {
            _buffer = new Buffers.Buffer(buffer, offset, sizeof(UInt32));
        }
    }
}
