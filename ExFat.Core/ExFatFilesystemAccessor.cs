namespace ExFat.Core
{
    using System;
    using System.IO;
    using IO;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level accessor
    /// </summary>
    public class ExFatFilesystemAccessor : IClusterReader, IPartitionReader
    {
        private readonly Stream _partitionStream;
        private readonly object _streamLock = new object();

        public ExFatBootSector BootSector { get; }
        public long SectorsPerCluster => 1 << BootSector.SectorsPerCluster.Value;
        public int BytesPerSector => 1 << BootSector.BytesPerSector.Value;

        public int BytesPerCluster => 1 << (BootSector.BytesPerSector.Value + BootSector.BytesPerSector.Value);

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
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public Stream OpenClusters(long startCluster, long? length = null)
        {
            return new ClusterStream(this, this, startCluster, length);
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

        public long GetNext(long cluster)
        {
            lock (_streamLock)
            {
                // TODO: optimize... A lot!
                var fatPage = GetFatPage(cluster);
                var clusterIndex = cluster % ClustersPerFatPage;
                return LittleEndian.ToUInt32(fatPage, (int) clusterIndex * sizeof(Int32));
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
