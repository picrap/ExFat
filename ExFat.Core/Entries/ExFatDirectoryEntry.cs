namespace ExFat.Core.Entries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Buffers;
    using Buffer = Buffers.Buffer;

    public class ExFatDirectoryEntry
    {
        protected internal Buffer Buffer { get; }
        public IValueProvider<ExFatDirectoryEntryType> EntryType { get; }

        /// <summary>
        /// Gets the offset in directory.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public long Offset { get; private set; }

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
        /// Creates a <see cref="ExFatDirectoryEntry" /> given a buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static ExFatDirectoryEntry Create(Buffer buffer, long offset)
        {
            var entry = Create(buffer);
            if (entry != null)
                entry.Offset = offset;
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

        public virtual void Update(ICollection<ExFatDirectoryEntry > secondaryEntries)
        { }

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
