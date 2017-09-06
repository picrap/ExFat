// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
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
        public IValueProvider<UInt32> CreationTimeStamp { get; }
        public IValueProvider<UInt32> LastWriteTimeStamp { get; }
        public IValueProvider<UInt32> LastAccessTimeStamp { get; }
        public IValueProvider<Byte> Creation10msIncrement { get; }
        public IValueProvider<Byte> LastWrite10msIncrement { get; }
        public IValueProvider<Byte> CreationTimeZoneOffset { get; }
        public IValueProvider<Byte> LastWriteTimeZoneOffset { get; }
        public IValueProvider<Byte> LastAccessTimeZoneOffset { get; }

        public IValueProvider<DateTime> CreationTime { get; }
        public IValueProvider<DateTime> LastWriteTime { get; }
        public IValueProvider<DateTime> LastAccessTime { get; }

        public IValueProvider<TimeSpan> CreationTimeOffset { get; }
        public IValueProvider<TimeSpan> LastWriteTimeOffset { get; }
        public IValueProvider<TimeSpan> LastAccessTimeOffset { get; }

        public IValueProvider<DateTimeOffset> CreationDateTimeOffset { get; }
        public IValueProvider<DateTimeOffset> LastWriteDateTimeOffset { get; }
        public IValueProvider<DateTimeOffset> LastAccessDateTimeOffset { get; }

        public FileExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            // the raw
            SecondaryCount = new BufferUInt8(buffer, 1);
            SetChecksum = new BufferUInt16(buffer, 2);
            FileAttributes = new EnumValueProvider<ExFatFileAttributes, UInt16>(new BufferUInt16(buffer, 4));
            CreationTimeStamp = new BufferUInt32(buffer, 8);
            LastWriteTimeStamp = new BufferUInt32(buffer, 12);
            LastAccessTimeStamp = new BufferUInt32(buffer, 16);
            Creation10msIncrement = new BufferUInt8(buffer, 20);
            LastWrite10msIncrement = new BufferUInt8(buffer, 21);
            CreationTimeZoneOffset = new BufferUInt8(buffer, 22);
            LastWriteTimeZoneOffset = new BufferUInt8(buffer, 23);
            LastAccessTimeZoneOffset = new BufferUInt8(buffer, 24);

            // the cooked
            CreationTime = new EntryDateTime(CreationTimeStamp, Creation10msIncrement);
            LastWriteTime = new EntryDateTime(LastWriteTimeStamp, LastWrite10msIncrement);
            LastAccessTime = new EntryDateTime(LastAccessTimeStamp);

            CreationTimeOffset = new EntryTimeZone(CreationTimeZoneOffset);
            LastWriteTimeOffset = new EntryTimeZone(LastWriteTimeZoneOffset);
            LastAccessTimeOffset = new EntryTimeZone(LastAccessTimeZoneOffset);

            CreationDateTimeOffset = new EntryDateTimeOffset(CreationTime, CreationTimeOffset);
            LastWriteDateTimeOffset = new EntryDateTimeOffset(LastWriteTime, LastWriteTimeOffset);
            LastAccessDateTimeOffset = new EntryDateTimeOffset(LastAccessTime, LastAccessTimeOffset);
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
