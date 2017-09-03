namespace ExFat.Core.Entries
{
    using System;
    using System.Diagnostics;
    using Buffers;
    using IO;
    using Buffer = Buffers.Buffer;

    [DebuggerDisplay("Allocation bitmap @{FirstCluster.Value} ({DataLength.Value})")]
    public class AllocationBitmapExFatDirectoryEntry : ExFatDirectoryEntry, IDataProvider
    {
        /// <summary>
        /// Gets the bitmap flags.
        /// </summary>
        /// <value>
        /// The bitmap flags.
        /// </value>
        public IValueProvider<Byte> BitmapFlags { get; }
        public IValueProvider<UInt32> FirstCluster { get; }
        public IValueProvider<UInt64> DataLength { get; }

        public DataDescriptor DataDescriptor => new DataDescriptor(FirstCluster.Value, false, DataLength.Value);

        public AllocationBitmapExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            BitmapFlags = new BufferUInt8(buffer, 1);
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}