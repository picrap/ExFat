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

        public int BytesPerCluster => (int) (BootSector.SectorsPerCluster.Value * BootSector.BytesPerSector.Value);

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

        public void Dispose()
        {
            Flush();
        }

        /// <summary>
        /// Flushes all pending changes.
        /// </summary>
        public void Flush()
        {
            FlushFatPage();
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
            return sectorIndex * (int) BootSector.BytesPerSector.Value;
        }

        private void SeekSector(long sectorIndex)
        {
            _partitionStream.Seek(GetSectorOffset(sectorIndex), SeekOrigin.Begin);
        }

        private long _fatPageIndex = -1;
        private byte[] _fatPage;
        private bool _fatPageDirty;
        private const int SectorsPerFatPage = 1;
        private int FatPageSize => (int) BootSector.BytesPerSector.Value * SectorsPerFatPage;
        private int ClustersPerFatPage => FatPageSize / sizeof(Int32);

        private byte[] GetFatPage(long cluster)
        {
            if (_fatPage == null)
                _fatPage = new byte[FatPageSize];

            var fatPageIndex = cluster / ClustersPerFatPage;
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

        public long GetNextCluster(long cluster)
        {
            // TODO: optimize?
            lock (_streamLock)
            {
                var fatPage = GetFatPage(cluster);
                var clusterIndex = (int) (cluster % ClustersPerFatPage);
                var nextCluster = LittleEndian.ToUInt32(fatPage, clusterIndex * sizeof(Int32));
                // consider this as signed
                if (nextCluster >= 0xFFFFFFF7)
                    return (int) nextCluster;
                // otherwise, it's the raw unsigned cluster number, extended to long
                return nextCluster;
            }
        }

        public void SetNextCluster(long cluster, long nextCluster)
        {
            lock (_streamLock)
            {
                var fatPage = GetFatPage(cluster);
                var clusterIndex = (int) (cluster % ClustersPerFatPage);
                LittleEndian.GetBytes((UInt32) nextCluster, fatPage, clusterIndex * sizeof(Int32));
                _fatPageDirty = true;
            }
        }

        /// <summary>
        /// Allocates a cluster.
        /// </summary>
        /// <param name="previousCluster">The previous cluster.</param>
        /// <returns></returns>
        public long AllocateCluster(long previousCluster)
        {
            var allocationBitmap = GetAllocationBitmap();
            lock (_streamLock)
            {
                var cluster = previousCluster + 1;
                if (allocationBitmap[cluster])
                    cluster = allocationBitmap.FindUnallocated();
                allocationBitmap[cluster] = true;
                return cluster;
            }
        }

        public void ReadCluster(long cluster, byte[] clusterBuffer, int offset, int length)
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

        public void WriteCluster(long cluster, byte[] clusterBuffer, int offset, int length)
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
                _partitionStream.Read(sectorBuffer, 0, (int) BootSector.BytesPerSector.Value * sectorCount);
            }
        }

        public void WriteSectors(long sector, byte[] sectorBuffer, int sectorCount)
        {
            lock (_streamLock)
            {
                SeekSector(sector);
                _partitionStream.Write(sectorBuffer, 0, (int) BootSector.BytesPerSector.Value * sectorCount);
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
                // TODO use Up case table
                var uc = upCaseTable.ToUpper(c);
                hash = (UInt16) (hash.RotateRight() + (uc & 0xFF));
                hash = (UInt16) (hash.RotateRight() + (uc >> 8));
            }
            return hash;
        }

        /// <summary>
        /// Opens a clusters stream.
        /// </summary>
        /// <param name="firstCluster">The first cluster.</param>
        /// <param name="contiguous">if set to <c>true</c> all stream clusters are contiguous (allowing a faster seek).</param>
        /// <param name="fileAccess">The file access.</param>
        /// <param name="length">The length (optional for non-contiguous cluster streams).</param>
        /// <param name="onDisposed">The on disposed.</param>
        /// <returns></returns>
        public ClusterStream OpenClusterStream(ulong firstCluster, bool contiguous, FileAccess fileAccess, ulong? length = null, Action onDisposed = null)
        {
            if (fileAccess == FileAccess.Read)
                return new ClusterStream(this, null, firstCluster, contiguous, length, onDisposed);
            // write and read/write will be the same
            return new ClusterStream(this, this, firstCluster, contiguous, length, onDisposed);
        }

        /// <summary>
        /// Opens the data stream.
        /// </summary>
        /// <param name="dataDescriptor">The data descriptor.</param>
        /// <param name="fileAccess">The file access.</param>
        /// <returns></returns>
        public ClusterStream OpenDataStream(DataDescriptor dataDescriptor, FileAccess fileAccess)
        {
            if (dataDescriptor == null)
                return null;
            return OpenClusterStream(dataDescriptor.FirstCluster, dataDescriptor.Contiguous, fileAccess, dataDescriptor.Length);
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
                    .First(b => !b.BitmapFlags.Value.HasFlag(AllocationBitmapFlags.SecondClusterBitmap));
                var allocationBitmapStream = OpenDataStream(allocationBitmapEntry.DataDescriptor, FileAccess.Read);
                _allocationBitmap.Open(allocationBitmapStream, allocationBitmapEntry.FirstCluster.Value, BootSector.ClusterCount.Value);
            }
            return _allocationBitmap;
        }
    }
}