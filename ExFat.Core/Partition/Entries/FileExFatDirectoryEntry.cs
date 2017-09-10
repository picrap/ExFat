// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Buffers;
    using Filesystem;
    using Buffer = Buffers.Buffer;

    /// <summary>
    /// Represents a directory entry for <see cref="ExFatEntryFilesystem"/>
    /// </summary>
    /// <seealso cref="ExFat.Partition.Entries.ExFatDirectoryEntry" />
    [DebuggerDisplay("File")]
    public class FileExFatDirectoryEntry : ExFatDirectoryEntry
    {
        /// <summary>
        /// Gets or sets the secondary entries count.
        /// </summary>
        /// <value>
        /// The secondary count.
        /// </value>
        public IValueProvider<Byte> SecondaryCount { get; }
        /// <summary>
        /// Gets or sets the set checksum.
        /// </summary>
        /// <value>
        /// The set checksum.
        /// </value>
        public IValueProvider<UInt16> SetChecksum { get; }
        /// <summary>
        /// Gets or sets the file attributes.
        /// </summary>
        /// <value>
        /// The file attributes.
        /// </value>
        public IValueProvider<ExFatFileAttributes> FileAttributes { get; }
        /// <summary>
        /// Gets or sets the creation time stamp.
        /// </summary>
        /// <value>
        /// The creation time stamp.
        /// </value>
        public IValueProvider<UInt32> CreationTimeStamp { get; }
        /// <summary>
        /// Gets or sets the last write time stamp.
        /// </summary>
        /// <value>
        /// The last write time stamp.
        /// </value>
        public IValueProvider<UInt32> LastWriteTimeStamp { get; }
        /// <summary>
        /// Gets or sets the last access time stamp.
        /// </summary>
        /// <value>
        /// The last access time stamp.
        /// </value>
        public IValueProvider<UInt32> LastAccessTimeStamp { get; }
        /// <summary>
        /// Gets or sets the creation time 10ms increment.
        /// </summary>
        /// <value>
        /// The creation10ms increment.
        /// </value>
        public IValueProvider<Byte> Creation10msIncrement { get; }
        /// <summary>
        /// Gets or sets the last write time 10ms increment.
        /// </summary>
        /// <value>
        /// The last write10ms increment.
        /// </value>
        public IValueProvider<Byte> LastWrite10msIncrement { get; }
        /// <summary>
        /// Gets or the creation time zone offset.
        /// </summary>
        /// <value>
        /// The creation time zone offset.
        /// </value>
        public IValueProvider<Byte> CreationTimeZoneOffset { get; }
        /// <summary>
        /// Gets or sets the last write time zone offset.
        /// </summary>
        /// <value>
        /// The last write time zone offset.
        /// </value>
        public IValueProvider<Byte> LastWriteTimeZoneOffset { get; }
        /// <summary>
        /// Gets or sets the last access time zone offset.
        /// </summary>
        /// <value>
        /// The last access time zone offset.
        /// </value>
        public IValueProvider<Byte> LastAccessTimeZoneOffset { get; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public IValueProvider<DateTime> CreationTime { get; }
        /// <summary>
        /// Gets or sets the last write time.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public IValueProvider<DateTime> LastWriteTime { get; }
        /// <summary>
        /// Gets or sets the last access time.
        /// </summary>
        /// <value>
        /// The last access time.
        /// </value>
        public IValueProvider<DateTime> LastAccessTime { get; }

        /// <summary>
        /// Gets or sets the creation time offset.
        /// </summary>
        /// <value>
        /// The creation time offset.
        /// </value>
        public IValueProvider<TimeSpan> CreationTimeOffset { get; }
        /// <summary>
        /// Gets or sets the last write time offset.
        /// </summary>
        /// <value>
        /// The last write time offset.
        /// </value>
        public IValueProvider<TimeSpan> LastWriteTimeOffset { get; }
        /// <summary>
        /// Gets or sets the last access time offset.
        /// </summary>
        /// <value>
        /// The last access time offset.
        /// </value>
        public IValueProvider<TimeSpan> LastAccessTimeOffset { get; }

        /// <summary>
        /// Gets or sets the creation date time offset.
        /// </summary>
        /// <value>
        /// The creation date time offset.
        /// </value>
        public IValueProvider<DateTimeOffset> CreationDateTimeOffset { get; }
        /// <summary>
        /// Gets or sets the last write date time offset.
        /// </summary>
        /// <value>
        /// The last write date time offset.
        /// </value>
        public IValueProvider<DateTimeOffset> LastWriteDateTimeOffset { get; }
        /// <summary>
        /// Gets or sets the last access date time offset.
        /// </summary>
        /// <value>
        /// The last access date time offset.
        /// </value>
        public IValueProvider<DateTimeOffset> LastAccessDateTimeOffset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExFatDirectoryEntry"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
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

        /// <summary>
        /// Updates the specified secondary entries.
        /// </summary>
        /// <param name="secondaryEntries">The secondary entries.</param>
        public override void Update(ICollection<ExFatDirectoryEntry> secondaryEntries)
        {
            SecondaryCount.Value = (Byte)secondaryEntries.Count;
            SetChecksum.Value = ComputeChecksum(secondaryEntries);
        }

        /// <summary>
        /// Computes the checksum.
        /// </summary>
        /// <param name="secondaryEntries">The secondary entries.</param>
        /// <returns></returns>
        public UInt16 ComputeChecksum(IEnumerable<ExFatDirectoryEntry> secondaryEntries)
        {
            var checksum = Buffer.Bytes.GetChecksum16(0, 2);
            checksum = Buffer.Bytes.GetChecksum16(4, 28, checksum);
            foreach (var secondaryEntry in secondaryEntries)
                checksum = secondaryEntry.Buffer.Bytes.GetChecksum16(0, 32, checksum);
            return checksum;
        }
    }
}
