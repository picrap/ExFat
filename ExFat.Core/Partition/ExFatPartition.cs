// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Entries;
    using IO;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level accessor
    /// </summary>
    public class ExFatPartition : IClusterWriter, IDisposable
    {
        private readonly Stream _partitionStream;
        private readonly object _streamLock = new object();

        public ExFatBootSector BootSector { get; }

        public int BytesPerCluster => (int)(BootSector.SectorsPerCluster.Value * BootSector.BytesPerSector.Value);

        public DataDescriptor RootDirectoryDataDescriptor => new DataDescriptor(BootSector.RootDirectoryCluster.Value, false, long.MaxValue);

        public ExFatPartition(Stream partitionStream)
        {
            if (!partitionStream.CanSeek)
                throw new ArgumentException("Given stream must be seekable");
            if (!partitionStream.CanRead)
                throw new ArgumentException("Given stream must be readable");

            _partitionStream = partitionStream;
            BootSector = ReadBootSector(_partitionStream);
        }

        public void Dispose()
        {
            Flush();
            DisposeAllocationBitmap();
        }

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
            FlushFatPage();
            // because .Flush() is not implemented in DiscUtils :)
            try
            {
                _partitionStream.Flush();
            }
            catch (NotImplementedException) { }
        }

        private void FlushAllocationBitmap()
        {
            _allocationBitmap?.Flush();
        }

        public static ExFatBootSector ReadBootSector(Stream partitionStream)
        {
            partitionStream.Seek(0, SeekOrigin.Begin);
            var bootSector = new ExFatBootSector();
            bootSector.Read(partitionStream);
            return bootSector;
        }

        public long GetClusterOffset(Cluster cluster)
        {
            return (BootSector.ClusterOffsetSector.Value + (cluster.Value - 2) * BootSector.SectorsPerCluster.Value) * BootSector.BytesPerSector.Value;
        }

        private void SeekCluster(Cluster cluster, long offset = 0)
        {
            _partitionStream.Seek(GetClusterOffset(cluster) + offset, SeekOrigin.Begin);
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
        private bool _fatPageDirty;
        private const int SectorsPerFatPage = 1;
        private int FatPageSize => (int)BootSector.BytesPerSector.Value * SectorsPerFatPage;
        private int ClustersPerFatPage => FatPageSize / sizeof(Int32);

        private byte[] GetFatPage(Cluster cluster)
        {
            if (_fatPage == null)
                _fatPage = new byte[FatPageSize];

            var fatPageIndex = cluster.Value / ClustersPerFatPage;
            if (fatPageIndex != _fatPageIndex)
            {
                FlushFatPage();
                ReadSectors(BootSector.FatOffsetSector.Value + fatPageIndex * SectorsPerFatPage, _fatPage, SectorsPerFatPage);
                _fatPageIndex = fatPageIndex;
            }

            return _fatPage;
        }

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

        public Cluster GetNextCluster(Cluster cluster)
        {
            // TODO: optimize?
            lock (_streamLock)
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
            var cluster = dataDescriptor.FirstCluster;
            var length = (long?)dataDescriptor.Length ?? long.MaxValue;
            for (long offset = 0; offset < length; offset += BytesPerCluster)
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

        public void SetNextCluster(Cluster cluster, Cluster nextCluster)
        {
            lock (_streamLock)
            {
                var fatPage = GetFatPage(cluster);
                var clusterIndex = (int)(cluster.Value % ClustersPerFatPage);
                LittleEndian.GetBytes((UInt32)nextCluster.Value, fatPage, clusterIndex * sizeof(Int32));
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
            var allocationBitmap = GetAllocationBitmap();
            lock (_streamLock)
            {
                Cluster cluster;
                // no data? anything else is good
                if (!previousClusterHint.IsData)
                    cluster = allocationBitmap.FindUnallocated();
                else
                {
                    // try next
                    cluster = previousClusterHint + 1;
                    if (allocationBitmap[cluster])
                        cluster = allocationBitmap.FindUnallocated();
                }
                allocationBitmap[cluster] = true;
                return cluster;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Frees the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public void FreeCluster(Cluster cluster)
        {
            lock (_streamLock)
            {
                GetAllocationBitmap()[cluster] = false;
            }
        }

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

        public void ReadSectors(long sector, byte[] sectorBuffer, int sectorCount)
        {
            lock (_streamLock)
            {
                SeekSector(sector);
                _partitionStream.Read(sectorBuffer, 0, (int)BootSector.BytesPerSector.Value * sectorCount);
            }
        }

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
            return OpenClusterStream(new DataDescriptor(0, true, 0), FileAccess.ReadWrite, onDisposed);
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
            return new ExFatDirectory(OpenDataStream(dataDescriptor, FileAccess.ReadWrite), true);
        }

        private IEnumerable<TDirectoryEntry> FindRootDirectoryEntries<TDirectoryEntry>()
            where TDirectoryEntry : ExFatDirectoryEntry
        {
            using (var rootDirectory = OpenDirectory(RootDirectoryDataDescriptor))
                return rootDirectory.GetEntries().OfType<TDirectoryEntry>();
        }

        private ExFatUpCaseTable _upCaseTable;

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

        private ExFatAllocationBitmap _allocationBitmap;

        public ExFatAllocationBitmap GetAllocationBitmap()
        {
            if (_allocationBitmap == null)
            {
                _allocationBitmap = new ExFatAllocationBitmap();
                var allocationBitmapEntry = FindRootDirectoryEntries<AllocationBitmapExFatDirectoryEntry>()
                    .First(b => !b.BitmapFlags.Value.HasAny(AllocationBitmapFlags.SecondClusterBitmap));
                var allocationBitmapStream = OpenDataStream(allocationBitmapEntry.DataDescriptor, FileAccess.ReadWrite);
                _allocationBitmap.Open(allocationBitmapStream, allocationBitmapEntry.FirstCluster.Value, BootSector.ClusterCount.Value);
            }
            return _allocationBitmap;
        }

        public void UpdateEntry(ExFatMetaDirectoryEntry entry)
        {
            lock (_streamLock)
            {
                var offset = entry.Primary.Position % BytesPerCluster;
                SeekCluster(entry.Primary.Cluster, offset);
                entry.Write(null, _partitionStream);
            }
        }

        public void Deallocate(DataDescriptor dataDescriptor)
        {
            var allocationBitmap = GetAllocationBitmap();
            foreach (var cluster in GetClusters(dataDescriptor))
                allocationBitmap[cluster] = false;
        }
    }
}