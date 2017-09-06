// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.IO
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Information about data in partition
    /// </summary>
    [DebuggerDisplay("@{FirstCluster} ({Length}) contiguous={Contiguous}")]
    public class DataDescriptor
    {
        /// <summary>
        /// Gets the first cluster of the data in partition.
        /// </summary>
        /// <value>
        /// The first cluster.
        /// </value>
        public ulong FirstCluster { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DataDescriptor"/> is contiguous.
        /// When data is contiguous there is no need to read the FAT information
        /// </summary>
        /// <value>
        ///   <c>true</c> if contiguous; otherwise, <c>false</c>.
        /// </value>
        public bool Contiguous { get; }

        /// <summary>
        /// Gets the length. Can be null if the data is not contiguous (so the length is marked by last cluster)
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong? Length { get; }

        public DataDescriptor(ulong firstCluster, bool contiguous, ulong? length)
        {
            if (contiguous && !length.HasValue)
                throw new ArgumentException("length must be provided for contiguous streams");
            FirstCluster = firstCluster;
            Contiguous = contiguous;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDescriptor"/> class.
        /// </summary>
        /// <param name="firstCluster">The first cluster.</param>
        /// <param name="contiguous">if set to <c>true</c> [contiguous].</param>
        /// <param name="length">The length.</param>
        internal DataDescriptor(UInt32 firstCluster, bool contiguous, ulong? length)
            : this(firstCluster == 0 || firstCluster == ~0u ? ~0ul : firstCluster, contiguous, length)
        {
        }
    }
}