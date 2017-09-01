namespace ExFat.Core.Buffer
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferUInt64 : BufferData
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public UInt64 Value
        {
            get { return BitConverter.ToUInt64(GetAll().FromLittleEndian(), 0); }
            set { Set(BitConverter.GetBytes(value).ToLittleEndian()); }
        }

        public BufferUInt64(byte[] buffer, int offset) : base(buffer, offset, sizeof(UInt64))
        {
        }
    }
}
