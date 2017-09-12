// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.IO;
    using Entries;
    using IO;
    using Buffer = Buffers.Buffer;

    partial class ExFatPartition
    {
        /// <summary>
        /// Formats the specified partition stream.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <param name="bytesPerSector">The bytes per sector.</param>
        /// <param name="sectorsPerCluster">The sectors per cluster.</param>
        /// <param name="volumeLabel">The volume label.</param>
        /// <param name="volumeSpace">The volume space.</param>
        /// <returns></returns>
        public static ExFatPartition Format(Stream partitionStream, uint bytesPerSector, uint sectorsPerCluster, string volumeLabel = null, ulong? volumeSpace = null)
        {
            var partition = new ExFatPartition(partitionStream, false);
            partitionStream.Seek(0, SeekOrigin.Begin);

            volumeSpace = volumeSpace ?? (ulong?)partitionStream.Length;
            var totalSectors = volumeSpace.Value / bytesPerSector;
            const uint fats = 1;
            const uint usedFats = fats;
            const uint bootSectors = 12;
            const uint bpbSectors = 2 * bootSectors;

            // create bootsector
            var bootSectorBytes = new byte[bytesPerSector * 12];
            var bootSector = new ExFatBootSector(bootSectorBytes);
            var totalClusters = (uint)(totalSectors - bpbSectors) / (usedFats * 4 / bytesPerSector + sectorsPerCluster);
            var sectorsPerFat = totalClusters * 4 / bytesPerSector;
            bootSector.JmpBoot.Set(ExFatBootSector.DefaultJmpBoot);
            bootSector.OemName.Value = ExFatBootSector.ExFatOemName;
            bootSector.VolumeLengthSectors.Value = totalSectors;
            bootSector.FatOffsetSector.Value = Align(bpbSectors, bytesPerSector);
            bootSector.FatLengthSectors.Value = Align(sectorsPerFat, bytesPerSector);
            bootSector.ClusterOffsetSector.Value = Align(bootSector.FatOffsetSector.Value + usedFats * bootSector.FatLengthSectors.Value, bytesPerSector);
            totalClusters = (uint)((volumeSpace.Value / bytesPerSector - bootSector.ClusterOffsetSector.Value) / sectorsPerCluster);
            bootSector.ClusterCount.Value = totalClusters;
            bootSector.VolumeSerialNumber.Value = (uint)new Random().Next();
            bootSector.FileSystemRevision.Value = 256;
            bootSector.VolumeFlags.Value = 0;
            bootSector.BytesPerSector.Value = bytesPerSector;
            bootSector.SectorsPerCluster.Value = sectorsPerCluster;
            bootSector.NumberOfFats.Value = (byte)fats;
            bootSector.DriveSelect.Value = 0x80;
            bootSector.PercentInUse.Value = 0xFF;
            for (int sectorIndex = 0; sectorIndex <= 8; sectorIndex++)
            {
                bootSectorBytes[sectorIndex * bytesPerSector + bytesPerSector - 2] = 0x55;
                bootSectorBytes[sectorIndex * bytesPerSector + bytesPerSector - 1] = 0xAA;
            }
            partition.BootSector = bootSector;

            // prepare FAT
            partition.SetNextCluster(0, Cluster.Marker);
            partition.SetNextCluster(1, Cluster.Last);
            var lastCluster = Cluster.First + totalClusters;
            for (var cluster = Cluster.First; cluster.Value < lastCluster.Value; cluster += 1)
                partition.SetNextCluster(cluster, Cluster.Free);

            // create allocation bitmap
            var allocationBitmapEntry = CreateAllocationBitmap(partition, totalClusters);

            // create root directory (cluster 2)
            var directoryDataDescriptor = new DataDescriptor(0, false, ulong.MaxValue);
            using (partition.OpenClusterStream(directoryDataDescriptor, FileAccess.ReadWrite, d => directoryDataDescriptor = new DataDescriptor(d.FirstCluster, d.Contiguous, long.MaxValue))) { }
            bootSector.RootDirectoryCluster.Value = directoryDataDescriptor.FirstCluster.ToUInt32();

            // boot sector is now complete
            var checksum = bootSector.ComputeChecksum();
            for (var offset = 11 * bytesPerSector; offset < 12 * bytesPerSector; offset += 4)
            {
                bootSectorBytes[offset] = checksum[0];
                bootSectorBytes[offset + 1] = checksum[1];
                bootSectorBytes[offset + 2] = checksum[2];
                bootSectorBytes[offset + 3] = checksum[3];
            }
            partition.WriteSectors(0, bootSectorBytes, (int)bootSectors);
            partition.WriteSectors(bootSectors, bootSectorBytes, (int)bootSectors);

            using (var directoryStream = partition.OpenClusterStream(directoryDataDescriptor, FileAccess.ReadWrite))
            {
                allocationBitmapEntry.Write(directoryStream);

                CreateUpCaseTable(partition, directoryStream);
                CreateVolumeLabel(directoryStream, volumeLabel);
            }
            partition.Flush();
            return partition;
        }

        private static uint Align(uint value, uint bytesPerSector)
        {
            var alignment = 4 << 10; // 4K alignmet
            if (bytesPerSector > alignment)
                return value;
            var r = alignment / bytesPerSector - 1;
            return (uint)((value + r) & ~r);
        }

        private static void CreateVolumeLabel(ClusterStream directoryStream, string volumeLabel)
        {
            if (volumeLabel == null)
                return;
            var volumeLabelEntry = new VolumeLabelExFatDirectoryEntry(new Buffer(new byte[32]));
            volumeLabelEntry.EntryType.Value = ExFatDirectoryEntryType.VolumeLabel | ExFatDirectoryEntryType.InUse;
            volumeLabelEntry.VolumeLabel = volumeLabel;
            volumeLabelEntry.Write(directoryStream);
        }

        private static AllocationBitmapExFatDirectoryEntry CreateAllocationBitmap(ExFatPartition partition, uint totalClusters)
        {
            var allocationBitmap = new ExFatAllocationBitmap();
            allocationBitmap.Open(null, 2, totalClusters);
            partition._allocationBitmap = allocationBitmap;

            var dataDescriptor = new DataDescriptor(0, false, 0);
            using (var allocationBitmapStream = partition.OpenClusterStream(dataDescriptor, FileAccess.ReadWrite, d => dataDescriptor = d))
                allocationBitmap.Write(allocationBitmapStream);
            var allocationBitmapEntry = new AllocationBitmapExFatDirectoryEntry(new Buffer(new byte[32]));
            allocationBitmapEntry.EntryType.Value = ExFatDirectoryEntryType.AllocationBitmap | ExFatDirectoryEntryType.InUse;
            allocationBitmapEntry.BitmapFlags.Value = 0;
            allocationBitmapEntry.FirstCluster.Value = (uint)dataDescriptor.FirstCluster.Value;
            allocationBitmapEntry.DataLength.Value = (totalClusters + 7) / 8;
            return allocationBitmapEntry;
        }

        private static void CreateUpCaseTable(ExFatPartition partition, Stream directoryStream)
        {
            var upCaseTable = new ExFatUpCaseTable();
            upCaseTable.SetDefault();
            partition._upCaseTable = upCaseTable;
            var upCaseTableDataDescriptor = new DataDescriptor(0, false, long.MaxValue);
            long length;
            UInt32 checksum;
            using (var upCaseStream = partition.OpenClusterStream(upCaseTableDataDescriptor, FileAccess.ReadWrite, d => upCaseTableDataDescriptor = d))
            {
                checksum = upCaseTable.Write(upCaseStream);
                length = upCaseStream.Position;
            }

            var upCaseTableEntry = new UpCaseTableExFatDirectoryEntry(new Buffer(new byte[32]));
            upCaseTableEntry.EntryType.Value = ExFatDirectoryEntryType.UpCaseTable | ExFatDirectoryEntryType.InUse;
            upCaseTableEntry.TableChecksum.Value = checksum;
            upCaseTableEntry.FirstCluster.Value = (uint)upCaseTableDataDescriptor.FirstCluster.Value;
            upCaseTableEntry.DataLength.Value = (ulong)length;
            upCaseTableEntry.Write(directoryStream);
        }
    }
}