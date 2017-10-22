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
    /// Directory entry for up-case table
    /// </summary>
    /// <seealso cref="T:ExFat.Partition.Entries.ExFatDirectoryEntry" />
    /// <seealso cref="T:ExFat.IO.IDataProvider" />
    [DebuggerDisplay("Up case table @{FirstCluster.Value} ({DataLength.Value})")]
    public class UpCaseTableExFatDirectoryEntry : ExFatDirectoryEntry, IDataProvider
    {
        /// <summary>
        /// Gets or sets the table checksum.
        /// </summary>
        /// <value>
        /// The table checksum.
        /// </value>
        public IValueProvider<UInt32> TableChecksum { get; }
        /// <summary>
        /// Gets or sets the first cluster.
        /// </summary>
        /// <value>
        /// The first cluster.
        /// </value>
        public IValueProvider<UInt32> FirstCluster { get; }
        /// <summary>
        /// Gets or sets the length of the data.
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
        /// Initializes a new instance of the <see cref="T:ExFat.Partition.Entries.UpCaseTableExFatDirectoryEntry" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public UpCaseTableExFatDirectoryEntry(Buffer buffer) : base(buffer)
        {
            TableChecksum = new BufferUInt32(buffer, 4);
            FirstCluster = new BufferUInt32(buffer, 20);
            DataLength = new BufferUInt64(buffer, 24);
        }
    }
}