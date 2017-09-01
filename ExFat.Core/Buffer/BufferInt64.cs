namespace ExFat.Core.Buffer
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferInt64 : BufferData
    {
        public Int64 Value
        {
            get { return BitConverter.ToInt64(GetAll().FromLittleEndian(), 0); }
            set { Set(BitConverter.GetBytes(value).ToLittleEndian()); }
        }

        public BufferInt64(byte[] buffer, int offset) : base(buffer, offset, sizeof(Int64))
        {
        }
    }
}
