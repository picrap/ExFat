namespace ExFat.Core.Entries
{
    using System;
    using Buffers;
    using Buffer = Buffers.Buffer;

    public class ExFatDirectoryEntry
    {
        public IValueProvider<ExFatDirectoryEntryType> EntryType { get; }

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

        public bool IsSecondary => (EntryType.Value & ExFatDirectoryEntryType.IsSecondary) != 0;
        public bool IsBenign => (EntryType.Value & ExFatDirectoryEntryType.IsBenign) != 0;

        protected ExFatDirectoryEntry(Buffer buffer)
        {
            EntryType = new EnumValueProvider<ExFatDirectoryEntryType, Byte>(new BufferUInt8(buffer, 0));
        }

        /// <summary>
        /// Creates a <see cref="ExFatDirectoryEntry"/> given a buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static ExFatDirectoryEntry Create(Buffer buffer)
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
    }
}
