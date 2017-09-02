namespace ExFat.Core.Entries
{
    using System;
    using System.Diagnostics;
    using Buffers;
    using Buffer = Buffers.Buffer;

    [DebuggerDisplay("Stream extension length={ValidDataLength.Value} @{FirstCluster.Value} ({DataLength.Value})")]
    public class StreamExtensionExFatDirectoryEntry : ExFatDirectoryEntry
    {
        public IValueProvider<ExFatGeneralSecondaryFlags> GeneralSecondaryFlags { get; }
        public IValueProvider<Byte> NameLength { get; }
        public IValueProvider<UInt16> NameHash { get; }
        public IValueProvider<UInt64> ValidDataLength { get; }
        /// <summary>
        /// Gets the first cluster MINUS 2 (WTF?).
        /// </summary>
        /// <value>
        /// The first cluster.
        /// </value>
        public IValueProvider<UInt32> FirstCluster { get; }
        public IValueProvider<UInt64> DataLength { get; }

        public StreamExtensionExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            GeneralSecondaryFlags = new EnumValueProvider<ExFatGeneralSecondaryFlags, Byte>(new BufferUInt8(buffer, 1));
            NameLength = new BufferUInt8(buffer, 3);
            NameHash = new BufferUInt16(buffer, 4);
            ValidDataLength = new BufferUInt64(buffer, 8);
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}