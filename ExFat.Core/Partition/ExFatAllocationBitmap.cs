// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.IO;
    using IO;

    /// <summary>
    /// Allocation bitmap manager. Allows to allocate or free clusters from bitmap.
    /// This is where you start when you want a free cluster, then it needs to be chained... If not contiguous
    /// </summary>
    public class ExFatAllocationBitmap
    {
        private byte[] _bitmap;
        private Stream _dataStream;
        private uint _firstCluster;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length { get; private set; }

        /// <summary>
        /// Gets or sets the allocation state for the specified cluster.
        /// </summary>
        /// <value>
        /// The <see cref="System.Boolean"/>.
        /// </value>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        public bool this[Cluster cluster]
        {
            get { return GetAt(cluster); }
            set { SetAt(cluster, value); }
        }

        /// <summary>
        /// Opens the specified data stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firstCluster">The first cluster.</param>
        /// <param name="length">The length.</param>
        public void Open(Stream dataStream, uint firstCluster, long length)
        {
            _dataStream = dataStream;
            _firstCluster = firstCluster;
            _bitmap = new byte[dataStream.Length];
            dataStream.Read(_bitmap, 0, _bitmap.Length);
            Length = length;
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            _dataStream.Flush();
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            Flush();
            _dataStream.Dispose();
        }

        /// <summary>
        /// Indicates whether the specified cluster is free
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">cluster</exception>
        public bool GetAt(Cluster cluster)
        {
            if (cluster.Value < _firstCluster || cluster.Value >= Length)
                throw new ArgumentOutOfRangeException(nameof(cluster));
            var clusterIndex = cluster.Value - _firstCluster;
            var byteIndex = (int)clusterIndex / 8;
            var bitMask = 1 << (int)(clusterIndex & 7);
            return (_bitmap[byteIndex] & bitMask) != 0;
        }

        /// <summary>
        /// Allocates or frees the specified cluster
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="allocated">if set to <c>true</c> [allocated].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">cluster</exception>
        public void SetAt(Cluster cluster, bool allocated)
        {
            if (cluster.Value < _firstCluster || cluster.Value >= Length)
                throw new ArgumentOutOfRangeException(nameof(cluster));
            var clusterIndex = cluster.Value - _firstCluster;
            var byteIndex = (int)clusterIndex / 8;
            var bitMask = 1 << (int)(clusterIndex & 7);
            if (allocated)
                _bitmap[byteIndex] |= (byte)bitMask;
            else
                _bitmap[byteIndex] &= (byte)~bitMask;
            // for some unknown reason, this does not work on DiscUtils, so the Flush() handle all problems
            _dataStream.Seek(byteIndex, SeekOrigin.Begin);
            _dataStream.Write(_bitmap, byteIndex, 1);
        }

        /// <summary>
        /// Finds one or more unallocated cluster.
        /// Does not allocate them, so all allocation process must be perform form within a lock
        /// </summary>
        /// <param name="contiguous">The contiguous.</param>
        /// <returns></returns>
        public Cluster FindUnallocated(int contiguous = 1)
        {
            UInt32 freeCluster = 0;
            int unallocatedCount = 0;
            for (UInt32 cluster = _firstCluster; cluster < Length;)
            {
                // special case: byte is filled, skip the block (and reset the search)
                if (((cluster - _firstCluster) & 0x07) == 0 && _bitmap[cluster / 8] == 0xFF)
                {
                    freeCluster = 0;
                    unallocatedCount = 0;
                    cluster += 8;
                    continue;
                }
                // if it's free, count it
                if (!GetAt(cluster))
                {
                    // first to be free, keep it
                    if (unallocatedCount == 0)
                        freeCluster = cluster;
                    unallocatedCount++;

                    // when the amount is reached, return it
                    if (unallocatedCount == contiguous)
                        return freeCluster;
                }
                ++cluster;
            }
            // nothing found
            return Cluster.Free;
        }
    }
}