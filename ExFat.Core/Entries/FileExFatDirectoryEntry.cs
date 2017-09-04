namespace ExFat.Core.Entries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Buffers;
    using Buffer = Buffers.Buffer;

    [DebuggerDisplay("File")]
    public class FileExFatDirectoryEntry : ExFatDirectoryEntry
    {
        public IValueProvider<Byte> SecondaryCount { get; }
        public IValueProvider<UInt16> SetChecksum { get; }
        public IValueProvider<ExFatFileAttributes> FileAttributes { get; }
        public IValueProvider<UInt32> CreationTime { get; }
        public IValueProvider<UInt32> LastModified { get; }
        public IValueProvider<UInt32> LastAccessed { get; }
        public IValueProvider<Byte> Create10msIncrement { get; }
        public IValueProvider<Byte> LastModified10msIncrement { get; }
        public IValueProvider<Byte> LastAccessed10msIncrement { get; }

        public IValueProvider<DateTime> CreationDateTime { get; }
        public IValueProvider<DateTime> LastModifiedDateTime { get; }
        public IValueProvider<DateTime> LastAccessedDateTime { get; }

        public FileExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            SecondaryCount = new BufferUInt8(buffer, 1);
            SetChecksum = new BufferUInt16(buffer, 2);
            FileAttributes = new EnumValueProvider<ExFatFileAttributes, UInt16>(new BufferUInt16(buffer, 4));
            CreationTime = new BufferUInt32(buffer, 8);
            LastModified = new BufferUInt32(buffer, 12);
            LastAccessed = new BufferUInt32(buffer, 16);
            Create10msIncrement = new BufferUInt8(buffer, 20);
            LastModified10msIncrement = new BufferUInt8(buffer, 21);
            LastAccessed10msIncrement = new BufferUInt8(buffer, 22);
            CreationDateTime = new EntryDateTime(CreationTime, Create10msIncrement);
            LastModifiedDateTime = new EntryDateTime(LastModified, LastModified10msIncrement);
            LastAccessedDateTime = new EntryDateTime(LastAccessed, LastAccessed10msIncrement);
        }

        public override void Update(ICollection<ExFatDirectoryEntry> secondaryEntries)
        {
            SecondaryCount.Value = (Byte)secondaryEntries.Count;
            SetChecksum.Value = ComputeChecksum(secondaryEntries);
        }

        public UInt16 ComputeChecksum(IEnumerable<ExFatDirectoryEntry> secondaryEntries)
        {
            var checksum = Buffer.Bytes.GetChecksum(0, 2);
            checksum = Buffer.Bytes.GetChecksum(4, 28, checksum);
            foreach (var secondaryEntry in secondaryEntries)
                checksum = secondaryEntry.Buffer.Bytes.GetChecksum(0, 32, checksum);
            return checksum;
        }
    }
}
