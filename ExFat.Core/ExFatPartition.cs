namespace ExFat.Core
{
    using System;
    using System.IO;
    using IO;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level accessor
    /// </summary>
    public class ExFatPartition : IClusterReader
    {
        private readonly Stream _partitionStream;
        private readonly object _streamLock = new object();

        public ExFatBootSector BootSector { get; }

        public int BytesPerCluster => (int)(BootSector.SectorsPerCluster.Value * BootSector.BytesPerSector.Value);

        public DataDescriptor RootDirectoryDataDescriptor => new DataDescriptor(BootSector.RootDirectoryCluster.Value, false, null);

        public ExFatPartition(Stream partitionStream)
        {
            if (!partitionStream.CanSeek)
                throw new ArgumentException("Given stream must be seekable");
            if (!partitionStream.CanRead)
                throw new ArgumentException("Given stream must be readable");

            _partitionStream = partitionStream;
            BootSector = ReadBootSector(_partitionStream);
        }

        public static ExFatBootSector ReadBootSector(Stream partitionStream)
        {
            partitionStream.Seek(0, SeekOrigin.Begin);
            var bootSector = new ExFatBootSector();
            bootSector.Read(partitionStream);
            return bootSector;
        }

        public long GetClusterOffset(long clusterIndex)
        {
            return (BootSector.ClusterOffsetSector.Value + (clusterIndex - 2) * BootSector.SectorsPerCluster.Value) * BootSector.BytesPerSector.Value;
        }

        private void SeekCluster(long clusterIndex)
        {
            _partitionStream.Seek(GetClusterOffset(clusterIndex), SeekOrigin.Begin);
        }

        public long GetSectorOffset(long sectorIndex)
        {
            return sectorIndex * (int)BootSector.BytesPerSector.Value;
        }

        private void SeekSector(long sectorIndex)
        {
            _partitionStream.Seek(GetSectorOffset(sectorIndex), SeekOrigin.Begin);
        }

        private long _fatPageIndex = -1;
        private byte[] _fatPage;
        private const int SectorsPerFatPage = 1;
        private int FatPageSize => (int)BootSector.BytesPerSector.Value * SectorsPerFatPage;
        private int ClustersPerFatPage => FatPageSize / sizeof(Int32);

        private byte[] GetFatPage(long cluster)
        {
            if (_fatPage == null)
                _fatPage = new byte[FatPageSize];

            var fatPageIndex = cluster / ClustersPerFatPage;
            if (fatPageIndex != _fatPageIndex)
            {
                ReadSectors(BootSector.FatOffsetSector.Value + fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
                _fatPageIndex = fatPageIndex;
            }
            return _fatPage;
        }

        public long GetNextCluster(long cluster)
        {
            // TODO: optimize... A lot!
            lock (_streamLock)
            {
                var actualCluster = cluster;
                var fatPage = GetFatPage(actualCluster);
                var clusterIndex = (int)(actualCluster % ClustersPerFatPage);
                var nextCluster = LittleEndian.ToUInt32(fatPage, clusterIndex * sizeof(Int32));
                // consider this as signed
                if (nextCluster >= 0xFFFFFFF7)
                    return (int)nextCluster;
                // otherwise, it's the raw unsigned cluster number, extended to long
                return nextCluster;
            }
        }

        public void ReadCluster(long cluster, byte[] clusterBuffer)
        {
            lock (_streamLock)
            {
                SeekCluster(cluster);
                _partitionStream.Read(clusterBuffer, 0, BytesPerCluster);
            }
        }

        public void ReadSectors(long sector, byte[] sectorBuffer, int sectorCount)
        {
            lock (_streamLock)
            {
                SeekSector(sector);
                _partitionStream.Read(sectorBuffer, 0, (int)BootSector.BytesPerSector.Value * sectorCount);
            }
        }

        /// <summary>
        /// Gets the name hash.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public UInt16 GetNameHash(string name)
        {
            UInt16 hash = 0;
            foreach (var c in name)
            {
                // TODO use Up case table
                var uc = char.ToUpper(c);
                hash = (UInt16)(hash.RotateRight() + (uc & 0XFF));
                hash = (UInt16)(hash.RotateRight() + (uc >> 8));
            }
            return hash;
        }

        /// <summary>
        /// Opens a clusters stream.
        /// </summary>
        /// <param name="firstCluster">The first cluster.</param>
        /// <param name="contiguous">if set to <c>true</c> all stream clusters are contiguous (allowing a faster seek).</param>
        /// <param name="length">The length (optional for non-contiguous cluster streams).</param>
        /// <returns></returns>
        public Stream OpenClusters(ulong firstCluster, bool contiguous, ulong? length = null)
        {
            return new ClusterStream(this, firstCluster, contiguous, length);
        }

        /// <summary>
        /// Opens the data stream.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <returns></returns>
        public Stream OpenDataStream(DataDescriptor dataDescriptor)
        {
            if (dataDescriptor == null)
                return null;
            return OpenClusters(dataDescriptor.FirstCluster, dataDescriptor.Contiguous, dataDescriptor.Length);
        }

        /// <summary>
        /// Opens a directory.
        /// Caution: this does not check that the given <see cref="DataDescriptor"/> matches a directory descriptor.
        /// (You're at low-level, dude)
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <returns></returns>
        public ExFatDirectory OpenDirectory(DataDescriptor dataDescriptor)
        {
            return new ExFatDirectory(OpenDataStream(dataDescriptor), true);
        }
    }
}
