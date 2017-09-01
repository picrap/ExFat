namespace ExFat.Core.Entries
{
    using System;
    using Buffers;
    using Buffer = Buffers.Buffer;

    public abstract class ExFatDirectoryEntry
    {
        public IValueProvider<ExFatDirectoryEntryType> EntryType { get; }

        public bool IsDeleted => ((int)EntryType.Value & 0x80) == 0;

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
            switch ((ExFatDirectoryEntryType)(buffer[0] & 0x7F))
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
