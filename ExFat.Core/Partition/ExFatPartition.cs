// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using Entries;
    using IO;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level accessor
    /// </summary>
    public partial class ExFatPartition : IClusterWriter, IDisposable
    {
        private readonly Stream _partitionStream;
        private readonly ExFatOptions _options;
        private readonly object _streamLock = new object();
        private readonly object _fatLock = new object();

        /// <summary>
        /// Gets the boot sector.
        /// </summary>
        /// <value>
        /// The boot sector.
        /// </value>
        public ExFatBootSector BootSector { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the cluster size, in bytes
        /// </summary>
        /// <value>
        /// The bytes per cluster.
        /// </value>
        public int BytesPerCluster => (int)(BootSector.SectorsPerCluster.Value * BootSector.BytesPerSector.Value);

        /// <summary>
        /// Gets the root directory data descriptor.
        /// </summary>
        /// <value>
        /// The root directory data descriptor.
        /// </value>
        public DataDescriptor RootDirectoryDataDescriptor => new DataDescriptor(BootSector.RootDirectoryCluster.Value, false, long.MaxValue, long.MaxValue);

        /// <summary>
        /// Gets the total size.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        public long TotalSpace => BootSector.ClusterCount.Value * BytesPerCluster;

        /// <summary>
        /// Gets the used space
        /// </summary>
        /// <value>
        /// The used space.
        /// </value>
        public long UsedSpace => GetUsedClusters() * BytesPerCluster;

        /// <summary>
        /// Gets the available size.
        /// </summary>
        /// <value>
        /// The available size.
        /// </value>
        public long AvailableSpace => TotalSpace - UsedSpace;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatPartition" /> class.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentException">Given stream must be seekable
        /// or
        /// Given stream must be readable</exception>
        public ExFatPartition(Stream partitionStream, ExFatOptions options = ExFatOptions.Default)
            : this(partitionStream, options, true)
        {
        }

        private ExFatPartition(Stream partitionStream, ExFatOptions options, bool readBootsector)
        {
            if (!partitionStream.CanSeek)
                throw new ArgumentException("Given stream must be seekable");
            if (!partitionStream.CanRead)
                throw new ArgumentException("Given stream must be readable");

            _partitionStream = partitionStream;
            _options = options;
            if (readBootsector)
                BootSector = ReadBootSector(_partitionStream);
        }

        /// <summary>
        /// Releases (actually flushes) all pending resources (which are actually already flushed).
        /// </summary>
        public void Dispose()
        {
            Flush();
            DisposeAllocationBitmap();
        }

        /// <summary>
        /// Disposes the allocation bitmap.
        /// </summary>
        private void DisposeAllocationBitmap()
        {
            _allocationBitmap?.Dispose();
        }

        /// <summary>
        /// Flushes all pending changes.
        /// </summary>
        public void Flush()
        {
            FlushAllocationBitmap();
            lock (_fatLock)
                FlushFatPage();
            FlushPartitionStream();
        }

        /// <summary>
        /// Flushes the partition stream.
        /// </summary>
        private void FlushPartitionStream()
        {
            // because .Flush() is not implemented in DiscUtils :)
            try
            {
                _partitionStream.Flush();
            }
            catch (SystemException) // because I want to catch the NotImplemented.Exception but R# considers it as a TO.DO
            {
            }
        }

        /// <summary>
        /// Flushes the allocation bitmap.
        /// </summary>
        private void FlushAllocationBitmap()
        {
            _allocationBitmap?.Flush();
        }

        /// <summary>
        /// Reads the boot sector.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <returns></returns>
        public static ExFatBootSector ReadBootSector(Stream partitionStream)
        {
            partitionStream.Seek(0, SeekOrigin.Begin);
            var defaultBootSector = new ExFatBootSector(new byte[512]);
            defaultBootSector.Read(partitionStream);
            var sectorSize = defaultBootSector.BytesPerSector.Value;

            // it probably not a valid exFAT boot sector, so don't dig any further
            if (sectorSize < 512 || sectorSize > 4096)
                return defaultBootSector;

            var fullData = new byte[sectorSize * 12];

            partitionStream.Seek(0, SeekOrigin.Begin);
            var bootSector = new ExFatBootSector(fullData);
            bootSector.Read(partitionStream);
            return bootSector;
        }

        /// <summary>
        /// Gets the cluster offset.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        public long GetClusterOffset(Cluster cluster)
        {
            return (BootSector.ClusterOffsetSector.Value + (cluster.Value - 2) * BootSector.SectorsPerCluster.Value) * BootSector.BytesPerSector.Value;
        }

        /// <summary>
        /// Seeks the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="offset">The offset.</param>
        // locked on _streamLock
        private void SeekCluster(Cluster cluster, long offset = 0)
        {
            _partitionStream.Seek(GetClusterOffset(cluster) + offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Gets the sector offset.
        /// </summary>
        /// <param name="sectorIndex">Index of the sector.</param>
        /// <returns></returns>
        public long GetSectorOffset(long sectorIndex)
        {
            return sectorIndex * (int)BootSector.BytesPerSector.Value;
        }

        /// <summary>
        /// Seeks the sector.
        /// </summary>
        /// <param name="sectorIndex">Index of the sector.</param>
        // locked on _streamLock
        private void SeekSector(long sectorIndex)
        {
            _partitionStream.Seek(GetSectorOffset(sectorIndex), SeekOrigin.Begin);
        }

        private long _fatPageIndex = -1;
        private byte[] _fatPage;
        private bool _fatPageDirty;
        private const int SectorsPerFatPage = 1;
        private int FatPageSize => (int)BootSector.BytesPerSector.Value * SectorsPerFatPage;
        private int ClustersPerFatPage => FatPageSize / sizeof(Int32);

        // already locked on _fatLock
        private byte[] GetFatPage(Cluster cluster)
        {
            var fatPageIndex = GetFatPageIndex(cluster);
            return GetFatPageFromIndex(fatPageIndex);
        }

        // already locked on _fatLock (except from Format but we don't care)
        private byte[] GetFatPageFromIndex(long fatPageIndex)
        {
            if (_fatPage == null)
                _fatPage = new byte[FatPageSize];

            if (fatPageIndex != _fatPageIndex)
            {
                FlushFatPage();
                ReadSectors(BootSector.FatOffsetSector.Value + fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
                _fatPageIndex = fatPageIndex;
            }
            return _fatPage;
        }

        private long GetFatPageIndex(Cluster cluster)
        {
            var fatPageIndex = cluster.Value / ClustersPerFatPage;
            return fatPageIndex;
        }

        // already locked on _fatLock
        private void FlushFatPage()
        {
            if (_fatPage != null && _fatPageDirty)
            {
                // write first fat
                WriteSectors(BootSector.FatOffsetSector.Value + _fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
                // optionnally update second
                if (BootSector.NumberOfFats.Value == 2)
                    WriteSectors(BootSector.FatOffsetSector.Value + BootSector.FatLengthSectors.Value + _fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
                _fatPageDirty = false;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the next cluster for a given cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        public Cluster GetNextCluster(Cluster cluster)
        {
            // TODO: optimize?
            lock (_fatLock)
            {
                var fatPage = GetFatPage(cluster);
                var clusterIndex = (int)(cluster.Value % ClustersPerFatPage);
                var nextCluster = LittleEndian.ToUInt32(fatPage, clusterIndex * sizeof(Int32));
                return nextCluster;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets all clusters from a <see cref="T:ExFat.IO.DataDescriptor" />.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <returns></returns>
        public IEnumerable<Cluster> GetClusters(DataDescriptor dataDescriptor)
        {
            lock (_fatLock)
            {
                var cluster = dataDescriptor.FirstCluster;
                var length = dataDescriptor.PhysicalLength;
                for (ulong offset = 0; offset < length; offset += (ulong)BytesPerCluster)
                {
                    if (cluster.IsLast)
                        yield break;
                    yield return cluster;
                    if (dataDescriptor.Contiguous)
                        cluster = cluster + 1;
                    else
                        cluster = GetNextCluster(cluster);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Sets the next cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="nextCluster">The next cluster.</param>
        public void SetNextCluster(Cluster cluster, Cluster nextCluster)
        {
            lock (_fatLock)
            {
                // the only point is to avoid writing to virtual hard disk, which may be sparse and handle sparseness more or less efficiently...
                var fatPage = GetFatPage(cluster);
                var clusterIndex = (int)(cluster.Value % ClustersPerFatPage);
                var b = LittleEndian.GetBytes((UInt32)nextCluster.Value);
                var fatPageOffset = clusterIndex * sizeof(UInt32);
                if (b[0] == fatPage[fatPageOffset] && b[1] == fatPage[fatPageOffset + 1] && b[2] == fatPage[fatPageOffset + 2] && b[3] == fatPage[fatPageOffset + 3])
                    return;
                Buffer.BlockCopy(b, 0, fatPage, fatPageOffset, sizeof(Int32));
                _fatPageDirty = true;
                if (!_options.HasAny(ExFatOptions.DelayWrite))
                    FlushFatPage();
            }
        }

        // invoked only from format, so considered as safe
        private void ClearFat()
        {
            var lastPageIndex = GetFatPageIndex(BootSector.ClusterCount.Value);
            for (var pageIndex = 0; pageIndex <= lastPageIndex; pageIndex++)
            {
                var fatPage = GetFatPageFromIndex(pageIndex);
                if (fatPage.All(b => b == 0))
                    continue;
                Array.Clear(fatPage, 0, fatPage.Length);
                _fatPageDirty = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Allocates a cluster.
        /// </summary>
        /// <param name="previousClusterHint">A hint about the previous cluster, this allows to allocate the next one, if available</param>
        /// <returns></returns>
        public Cluster AllocateCluster(Cluster previousClusterHint)
        {
            // ExFatAllocationBitmap is thread-safe, so no need to lock here
            var allocationBitmap = GetAllocationBitmap();
            return allocationBitmap.Allocate(previousClusterHint);
        }

        /// <summary>
        /// Frees the specified <see cref="DataDescriptor"/> clusters.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        public void Free(DataDescriptor dataDescriptor)
        {
            // ExFatAllocationBitmap is thread-safe, so no need to lock here
            var allocationBitmap = GetAllocationBitmap();
            // TODO: optimize to write all only once
            foreach (var cluster in GetClusters(dataDescriptor))
                allocationBitmap.Free(cluster);
        }

        /// <inheritdoc />
        /// <summary>
        /// Frees the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public void FreeCluster(Cluster cluster)
        {
            // ExFatAllocationBitmap is thread-safe, so no need to lock here
            GetAllocationBitmap().Free(cluster);
        }

        /// <inheritdoc />
        /// <summary>
        /// Reads one cluster.
        /// </summary>
        /// <param name="cluster">The cluster number.</param>
        /// <param name="clusterBuffer">The cluster buffer. It must be large enough to contain full cluster</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// length
        /// or
        /// length
        /// or
        /// offset
        /// </exception>
        public void ReadCluster(Cluster cluster, byte[] clusterBuffer, int offset, int length)
        {
            if (length + offset > BytesPerCluster)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            lock (_streamLock)
            {
                SeekCluster(cluster);
                _partitionStream.Read(clusterBuffer, offset, length);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Writes the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="clusterBuffer">The cluster buffer.</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// length
        /// or
        /// length
        /// or
        /// offset
        /// </exception>
        public void WriteCluster(Cluster cluster, byte[] clusterBuffer, int offset, int length)
        {
            if (length + offset > BytesPerCluster)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            lock (_streamLock)
            {
                SeekCluster(cluster);
                _partitionStream.Write(clusterBuffer, offset, length);
            }
        }

        /// <summary>
        /// Reads the sectors.
        /// </summary>
        /// <param name="sector">The sector.</param>
        /// <param name="sectorBuffer">The sector buffer.</param>
        /// <param name="sectorCount">The sector count.</param>
        public void ReadSectors(long sector, byte[] sectorBuffer, int sectorCount)
        {
            lock (_streamLock)
            {
                SeekSector(sector);
                _partitionStream.Read(sectorBuffer, 0, (int)BootSector.BytesPerSector.Value * sectorCount);
            }
        }

        /// <summary>
        /// Writes the sectors.
        /// </summary>
        /// <param name="sector">The sector.</param>
        /// <param name="sectorBuffer">The sector buffer.</param>
        /// <param name="sectorCount">The sector count.</param>
        public void WriteSectors(long sector, byte[] sectorBuffer, int sectorCount)
        {
            lock (_streamLock)
            {
                SeekSector(sector);
                _partitionStream.Write(sectorBuffer, 0, (int)BootSector.BytesPerSector.Value * sectorCount);
            }
        }

        /// <summary>
        /// Gets the name hash.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public UInt16 ComputeNameHash(string name)
        {
            UInt16 hash = 0;
            var upCaseTable = GetUpCaseTable();
            foreach (var c in name)
            {
                var uc = upCaseTable.ToUpper(c);
                hash = (UInt16)(hash.RotateRight() + (uc & 0xFF));
                hash = (UInt16)(hash.RotateRight() + (uc >> 8));
            }
            return hash;
        }

        /// <summary>
        /// Opens a clusters stream.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <param name="fileAccess">The file access.</param>
        /// <param name="onDisposed">Method invoked when stream is disposed.</param>
        /// <returns></returns>
        private ClusterStream OpenClusterStream(DataDescriptor dataDescriptor, FileAccess fileAccess, Action<DataDescriptor> onDisposed = null)
        {
            if (fileAccess == FileAccess.Read)
                return new ClusterStream(this, null, dataDescriptor, onDisposed);
            // write and read/write will be the same
            return new ClusterStream(this, this, dataDescriptor, onDisposed);
        }

        /// <summary>
        /// Opens the data stream.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <param name="fileAccess">The file access.</param>
        /// <param name="onDisposed">Method invoked when stream is disposed.</param>
        /// <returns></returns>
        public ClusterStream OpenDataStream(DataDescriptor dataDescriptor, FileAccess fileAccess, Action<DataDescriptor> onDisposed = null)
        {
            if (dataDescriptor == null)
                return null;
            return OpenClusterStream(dataDescriptor, fileAccess, onDisposed);
        }

        /// <summary>
        /// Creates the data stream.
        /// </summary>
        /// <param name="onDisposed">The on disposed.</param>
        /// <returns></returns>
        public ClusterStream CreateDataStream(Action<DataDescriptor> onDisposed = null)
        {
            return OpenClusterStream(new DataDescriptor(0, true, 0, 0), FileAccess.ReadWrite, onDisposed);
        }

        private IEnumerable<TDirectoryEntry> FindRootDirectoryEntries<TDirectoryEntry>()
            where TDirectoryEntry : ExFatDirectoryEntry
        {
            return GetEntries(RootDirectoryDataDescriptor).OfType<TDirectoryEntry>();
        }

        private ExFatUpCaseTable _upCaseTable;

        /// <summary>
        /// Gets up case table.
        /// </summary>
        /// <returns></returns>
        public ExFatUpCaseTable GetUpCaseTable()
        {
            if (_upCaseTable == null)
            {
                _upCaseTable = new ExFatUpCaseTable();
                var upCaseTableEntry = FindRootDirectoryEntries<UpCaseTableExFatDirectoryEntry>().FirstOrDefault();
                if (upCaseTableEntry != null)
                {
                    using (var upCaseTableStream = OpenDataStream(upCaseTableEntry.DataDescriptor, FileAccess.Read))
                        _upCaseTable.Read(upCaseTableStream);
                }
                else
                    _upCaseTable.SetDefault();
            }
            return _upCaseTable;
        }

        private readonly object _allocationBitmapLock = new object();
        private ExFatAllocationBitmap _allocationBitmap;

        /// <summary>
        /// Gets the allocation bitmap.
        /// </summary>
        /// <returns></returns>
        public ExFatAllocationBitmap GetAllocationBitmap()
        {
            lock (_allocationBitmapLock)
            {
                if (_allocationBitmap == null)
                {
                    _allocationBitmap = new ExFatAllocationBitmap();
                    var allocationBitmapEntry = FindRootDirectoryEntries<AllocationBitmapExFatDirectoryEntry>()
                        .First(b => !b.BitmapFlags.Value.HasAny(AllocationBitmapFlags.SecondClusterBitmap));
                    var allocationBitmapStream = OpenDataStream(allocationBitmapEntry.DataDescriptor, FileAccess.ReadWrite);
                    _allocationBitmap.Open(allocationBitmapStream, allocationBitmapEntry.FirstCluster.Value, BootSector.ClusterCount.Value,
                        _options.HasAny(ExFatOptions.DelayWrite));
                }

                return _allocationBitmap;
            }
        }

        /// <summary>
        /// Gets the used clusters.
        /// </summary>
        /// <returns></returns>
        private long GetUsedClusters()
        {
            return GetAllocationBitmap().GetUsedClusters();
        }
    }
}