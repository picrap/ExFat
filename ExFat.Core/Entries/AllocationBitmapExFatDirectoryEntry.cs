namespace ExFat.Core.Entries
{
    using System;
    using Buffers;
    using Buffer = Buffers.Buffer;

    public class AllocationBitmapExFatDirectoryEntry : ExFatDirectoryEntry
    {
        public IValueProvider<Byte> BitmapFlags { get; }
        public IValueProvider<UInt32> FirstCluster { get; }
        public IValueProvider<UInt64> DataLength { get; }

        public AllocationBitmapExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            BitmapFlags = new BufferUInt8(buffer, 1);
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}