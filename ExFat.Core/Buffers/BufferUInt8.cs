namespace ExFat.Core.Buffers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt8 : IValueProvider<Byte>
    {
        private readonly Buffer _buffer;

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

        public BufferUInt8(Buffers.Buffer buffer, int offset)
        {
            _buffer = new Buffers.Buffer(buffer, offset, 1);
        }
    }
}
