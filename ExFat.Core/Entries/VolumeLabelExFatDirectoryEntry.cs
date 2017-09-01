namespace ExFat.Core.Entries
{
    using System;
    using Buffers;
    using Buffer = Buffers.Buffer;

    public class VolumeLabelExFatDirectoryEntry : ExFatDirectoryEntry
    {
        public IValueProvider<Byte> CharacterCount { get; }
        public IValueProvider<string> AllVolumeLabel { get; }

        public string VolumeLabel
        {
            get { return AllVolumeLabel.Value.Substring(0, CharacterCount.Value); }
            set
            {
                CharacterCount.Value = (byte)value.Length;
                AllVolumeLabel.Value = value;
            }
        }

        public VolumeLabelExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            CharacterCount = new BufferUInt8(buffer, 1);
            AllVolumeLabel = new BufferWideString(buffer, 2, 11);
        }
    }
}
