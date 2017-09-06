// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Buffers;
    using IO;
    using Buffer = Buffers.Buffer;

    public class ExFatDirectoryEntry
    {
        protected internal Buffer Buffer { get; }
        public IValueProvider<ExFatDirectoryEntryType> EntryType { get; }

        /// <summary>
        /// Gets the position in directory stream.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public long Position { get; private set; }

        /// <summary>
        /// Gets the cluster.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        public Cluster Cluster { get; private set; }

        /// <summary>
        /// Indicates if the entry is in use
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in use]; otherwise, <c>false</c>.
        /// </value>
        public bool InUse
        {
            get { return (EntryType.Value & ExFatDirectoryEntryType.InUse) != 0; }
            set
            {
                if (value)
                    EntryType.Value |= ExFatDirectoryEntryType.InUse;
                else
                    EntryType.Value &= ~ExFatDirectoryEntryType.InUse;
            }
        }

        public bool IsSecondary => (EntryType.Value & ExFatDirectoryEntryType.Secondary) != 0;
        public bool IsBenign => (EntryType.Value & ExFatDirectoryEntryType.Benign) != 0;

        protected ExFatDirectoryEntry(Buffer buffer)
        {
            Buffer = buffer;
            EntryType = new EnumValueProvider<ExFatDirectoryEntryType, Byte>(new BufferUInt8(buffer, 0));
        }

        /// <summary>
        /// Sets the position (updated when writing).
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="cluster">The cluster.</param>
        public void SetPosition(long position, Cluster cluster)
        {
            Position = position;
            Cluster = cluster;
        }

        /// <summary>
        /// Creates a <see cref="ExFatDirectoryEntry" /> given a buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static ExFatDirectoryEntry Create(Buffer buffer, long offset, Cluster cluster)
        {
            var entry = Create(buffer);
            if (entry != null)
            {
                entry.Position = offset;
                entry.Cluster = cluster;
            }
            return entry;
        }

        private static ExFatDirectoryEntry Create(Buffer buffer)
        {
            switch ((ExFatDirectoryEntryType)(buffer[0] & (byte)~ExFatDirectoryEntryType.InUse))
            {
                case 0:
                    return null;
                case ExFatDirectoryEntryType.AllocationBitmap:
                    return new AllocationBitmapExFatDirectoryEntry(buffer);
                case ExFatDirectoryEntryType.UpCaseTable:
                    return new UpCaseTableExFatDirectoryEntry(buffer);
                case ExFatDirectoryEntryType.VolumeLabel:
                    return new VolumeLabelExFatDirectoryEntry(buffer);
                case ExFatDirectoryEntryType.File:
                    return new FileExFatDirectoryEntry(buffer);
                case ExFatDirectoryEntryType.Stream:
                    return new StreamExtensionExFatDirectoryEntry(buffer);
                case ExFatDirectoryEntryType.FileName:
                    return new FileNameExtensionExFatDirectoryEntry(buffer);
                default:
                    // unhandled entries
                    return new ExFatDirectoryEntry(buffer);
            }
        }

        public virtual void Update(ICollection<ExFatDirectoryEntry> secondaryEntries)
        {
        }

        /// <summary>
        /// Writes the instance to specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Write(Stream stream)
        {
            stream.Write(Buffer.Bytes, 0, Buffer.Bytes.Length);
        }
    }
}