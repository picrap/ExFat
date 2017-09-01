namespace ExFat.Core.Buffers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt16 : IValueProvider<UInt16>
    {
        private readonly Buffer _buffer;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public UInt16 Value
        {
            get { return LittleEndian.ToUInt16(_buffer); }
            set { LittleEndian.GetBytes(value, _buffer); }
        }

        public BufferUInt16(Buffers.Buffer buffer, int offset)
        {
            _buffer = new Buffers.Buffer(buffer, offset, sizeof(UInt16));
        }
    }
}
