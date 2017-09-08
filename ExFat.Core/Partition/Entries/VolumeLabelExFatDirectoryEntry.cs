// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using System.Diagnostics;
    using Buffers;
    using Buffer = Buffers.Buffer;

    /// <summary>
    /// Volume label
    /// </summary>
    /// <seealso cref="ExFat.Partition.Entries.ExFatDirectoryEntry" />
    [DebuggerDisplay("Volume label {" + nameof(VolumeLabel) + "}")]
    public class VolumeLabelExFatDirectoryEntry : ExFatDirectoryEntry
    {
        /// <summary>
        /// Gets the volume label length, in characters.
        /// </summary>
        /// <value>
        /// The character count.
        /// </value>
        public IValueProvider<Byte> CharacterCount { get; }

        /// <summary>
        /// Full length volume label.
        /// </summary>
        /// <value>
        /// All volume label.
        /// </value>
        public IValueProvider<string> AllVolumeLabel { get; }

        /// <summary>
        /// Gets or sets the volume label.
        /// </summary>
        /// <value>
        /// The volume label.
        /// </value>
        public string VolumeLabel
        {
            get { return AllVolumeLabel.Value.Substring(0, CharacterCount.Value); }
            set
            {
                CharacterCount.Value = (byte) value.Length;
                AllVolumeLabel.Value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeLabelExFatDirectoryEntry"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public VolumeLabelExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            CharacterCount = new BufferUInt8(buffer, 1);
            AllVolumeLabel = new BufferWideString(buffer, 2, 11);
        }
    }
}