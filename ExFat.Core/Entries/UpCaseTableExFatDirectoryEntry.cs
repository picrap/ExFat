namespace ExFat.Core.Entries
{
    using System;
    using System.Diagnostics;
    using Buffers;
    using Buffer = Buffers.Buffer;

    [DebuggerDisplay("Up case table @{FirstCluster.Value} ({DataLength.Value})")]
    public class UpCaseTableExFatDirectoryEntry : ExFatDirectoryEntry
    {
        public IValueProvider<UInt32> TableChecksum { get; }
        public IValueProvider<UInt32> FirstCluster { get; }
        public IValueProvider<UInt64> DataLength { get; }

        public UpCaseTableExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            TableChecksum = new BufferUInt32(buffer, 4);
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}
