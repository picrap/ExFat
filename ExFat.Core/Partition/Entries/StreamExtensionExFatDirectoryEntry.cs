// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using System.Diagnostics;
    using Buffers;
    using IO;
    using Buffer = Buffers.Buffer;

    /// <summary>
    /// Stream secondary entry
    /// </summary>
    /// <seealso cref="ExFat.Partition.Entries.ExFatDirectoryEntry" />
    /// <seealso cref="ExFat.IO.IDataProvider" />
    [DebuggerDisplay("Stream extension length={ValidDataLength.Value} @{FirstCluster.Value} ({DataLength.Value})")]
    public class StreamExtensionExFatDirectoryEntry : ExFatDirectoryEntry, IDataProvider
    {
        /// <summary>
        /// Gets or sets the general secondary flags.
        /// </summary>
        /// <value>
        /// The general secondary flags.
        /// </value>
        public IValueProvider<ExFatGeneralSecondaryFlags> GeneralSecondaryFlags { get; }
        /// <summary>
        /// Gets or sets the length of the name.
        /// </summary>
        /// <value>
        /// The length of the name.
        /// </value>
        public IValueProvider<Byte> NameLength { get; }
        /// <summary>
        /// Gets or sets the name hash.
        /// </summary>
        /// <value>
        /// The name hash.
        /// </value>
        public IValueProvider<UInt16> NameHash { get; }
        /// <summary>
        /// Gets the length of the valid data.
        /// </summary>
        /// <value>
        /// The length of the valid data.
        /// This is lower than or equal to <see cref="DataLength"/>
        /// </value>
        public IValueProvider<UInt64> ValidDataLength { get; }

        /// <summary>
        /// Gets or sets the first cluster
        /// </summary>
        /// <value>
        /// The first cluster.
        /// </value>
        public IValueProvider<UInt32> FirstCluster { get; }

        /// <summary>
        /// Gets or sets the length of the data.
        /// This is the allocated data length.
        /// </summary>
        /// <value>
        /// The length of the data.
        /// </value>
        public IValueProvider<UInt64> DataLength { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the data descriptor.
        /// </summary>
        /// <value>
        /// The data descriptor or null if none found.
        /// </value>
        public DataDescriptor DataDescriptor
        {
            get { return new DataDescriptor(FirstCluster.Value, GeneralSecondaryFlags.Value.HasAny(ExFatGeneralSecondaryFlags.NoFatChain), DataLength.Value, ValidDataLength.Value); }
            set
            {
                FirstCluster.Value = value.FirstCluster.ToUInt32();
                if (value.Contiguous)
                    GeneralSecondaryFlags.Value |= ExFatGeneralSecondaryFlags.NoFatChain;
                else
                    GeneralSecondaryFlags.Value &= ~ExFatGeneralSecondaryFlags.NoFatChain;
                DataLength.Value = value.PhysicalLength;
                ValidDataLength.Value = value.LogicalLength;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamExtensionExFatDirectoryEntry"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
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