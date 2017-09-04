namespace ExFat.Core
{
    using System;
    using System.IO;

    public class ExFatAllocationBitmap
    {
        private byte[] _bitmap;
        private Stream _dataStream;
        private uint _firstCluster;

        public long Length { get; private set; }

        /// <summary>
        /// Gets or sets the allocation state fpr the specified cluster.
        /// </summary>
        /// <value>
        /// The <see cref="System.Boolean"/>.
        /// </value>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        public bool this[long cluster]
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

        public bool GetAt(long cluster)
        {
            if (cluster < _firstCluster || cluster >= Length)
                throw new ArgumentOutOfRangeException(nameof(cluster));
            cluster -= _firstCluster;
            var byteIndex = (int)cluster / 8;
            var bitMask = 1 << (int)(cluster & 7);
            return (_bitmap[byteIndex] & bitMask) != 0;
        }

        public void SetAt(long cluster, bool allocated)
        {
            if (cluster < _firstCluster || cluster >= Length)
                throw new ArgumentOutOfRangeException(nameof(cluster));
            cluster -= _firstCluster;
            var byteIndex = (int)cluster / 8;
            var bitMask = 1 << (int)(cluster & 7);
            if (allocated)
                _bitmap[byteIndex] |= (byte)bitMask;
            else
                _bitmap[byteIndex] &= (byte)~bitMask;
        }

        public long FindUnallocated(int contiguous = 1)
        {
            long freeCluster = -1;
            int unallocatedCount = 0;
            for (long cluster = _firstCluster; cluster < Length;)
            {
                // special case: byte is filled, skip the block (and reset the search)
                if (((cluster - _firstCluster) & 0x07) == 0 && _bitmap[cluster / 8] == 0xFF)
                {
                    freeCluster = -1;
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
            return 0;
        }
    }
}
