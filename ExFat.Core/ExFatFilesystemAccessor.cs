namespace ExFat.Core
{
    using System;
    using System.IO;
    using IO;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level accessor
    /// </summary>
    public class ExFatFilesystemAccessor : IClusterReader
    {
        private readonly Stream _partitionStream;
        private readonly object _streamLock = new object();

        public ExFatBootSector BootSector { get; }
        public long SectorsPerCluster => 1 << BootSector.SectorsPerCluster.Value;
        public int BytesPerSector => 1 << BootSector.BytesPerSector.Value;

        public int BytesPerCluster => 1 << (BootSector.SectorsPerCluster.Value + BootSector.BytesPerSector.Value);

        public ExFatFilesystemAccessor(Stream partitionStream)
        {
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

        /// <summary>
        /// Opens a clusters stream.
        /// </summary>
        /// <param name="startCluster">The start cluster.</param>
        /// <param name="contiguous">if set to <c>true</c> all stream clusters are contiguous (allowing a faster seek).</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public Stream OpenClusters(long startCluster, bool contiguous, ulong? length = null)
        {
            return new ClusterStream(this, startCluster, contiguous, length);
        }

        public long GetClusterOffset(long clusterIndex)
        {
            return (BootSector.ClusterOffset.Value + (clusterIndex - 2) * SectorsPerCluster) * BytesPerSector;
        }

        private void SeekCluster(long clusterIndex)
        {
            _partitionStream.Seek(GetClusterOffset(clusterIndex), SeekOrigin.Begin);
        }

        public long GetSectorOffset(long sectorIndex)
        {
            return sectorIndex * BytesPerSector;
        }

        private void SeekSector(long sectorIndex)
        {
            _partitionStream.Seek(GetSectorOffset(sectorIndex), SeekOrigin.Begin);
        }

        private long _fatPageIndex = -1;
        private byte[] _fatPage;
        private const int SectorsPerFatPage = 1;
        private int FatPageSize => BytesPerSector * SectorsPerFatPage;
        private int ClustersPerFatPage => FatPageSize / sizeof(Int32);

        private byte[] GetFatPage(long cluster)
        {
            if (_fatPage == null)
                _fatPage = new byte[FatPageSize];

            var fatPageIndex = cluster / ClustersPerFatPage;
            if (fatPageIndex != _fatPageIndex)
            {
                ReadSectors(BootSector.FatOffset.Value + fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
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
                _partitionStream.Read(sectorBuffer, 0, BytesPerSector * sectorCount);
            }
        }
    }
}
