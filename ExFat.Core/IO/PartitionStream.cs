// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.IO
{
    using System.IO;

    public abstract class PartitionStream : Stream
    {
        /// <summary>
        /// Gets the current cluster.
        /// </summary>
        /// <value>
        /// The cluster position.
        /// </value>
        public abstract long ClusterPosition { get; }

        /// <summary>
        /// Gets the offset in current cluster.
        /// </summary>
        /// <value>
        /// The cluster offset.
        /// </value>
        public abstract int ClusterOffset { get; }
    }
}