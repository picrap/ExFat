namespace ExFat.Core.Buffer
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt32 : BufferData
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public UInt64 Value
        {
            get { return BitConverter.ToUInt32(GetAll().FromLittleEndian(), 0); }
            set { Set(BitConverter.GetBytes(value).ToLittleEndian()); }
        }

        public BufferUInt32(byte[] buffer, int offset) : base(buffer, offset, sizeof(UInt32))
        {
        }
    }
}
