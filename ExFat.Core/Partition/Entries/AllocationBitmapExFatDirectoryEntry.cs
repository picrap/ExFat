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
    /// Allocation bitmap directory entry
    /// </summary>
    /// <seealso cref="ExFat.Partition.Entries.ExFatDirectoryEntry" />
    /// <seealso cref="ExFat.IO.IDataProvider" />
    [DebuggerDisplay("Allocation bitmap @{FirstCluster.Value} ({DataLength.Value})")]
    public class AllocationBitmapExFatDirectoryEntry : ExFatDirectoryEntry, IDataProvider
    {
        /// <summary>
        /// Gets the bitmap flags.
        /// </summary>
        /// <value>
        /// The bitmap flags.
        /// </value>
        public IValueProvider<AllocationBitmapFlags> BitmapFlags { get; }

        /// <summary>
        /// Gets or sets the first cluster.
        /// </summary>
        /// <value>
        /// The first cluster.
        /// </value>
        public IValueProvider<UInt32> FirstCluster { get; }
        /// <summary>
        /// Gets the length of the data.
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
        public DataDescriptor DataDescriptor => new DataDescriptor(FirstCluster.Value, false, DataLength.Value, DataLength.Value);

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.Partition.Entries.AllocationBitmapExFatDirectoryEntry" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public AllocationBitmapExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            BitmapFlags = new EnumValueProvider<AllocationBitmapFlags, Byte>(new BufferUInt8(buffer, 1));
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}