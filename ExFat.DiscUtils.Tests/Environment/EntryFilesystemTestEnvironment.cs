// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Environment
{
    using System;
    using System.IO;
    using System.Linq;
    using Filesystem;
    using global::DiscUtils;
    using global::DiscUtils.Partitions;
    using global::DiscUtils.Streams;
    using global::DiscUtils.Vhdx;

    internal class EntryFilesystemTestEnvironment : TestEnvironment
    {
        public ExFatEntryFilesystem FileSystem { get; private set; }

        public static EntryFilesystemTestEnvironment FromNewVhdx(bool allowKeepDebug = false, long length = 10L << 30)
        {
            var testEnvironment = new EntryFilesystemTestEnvironment();
            testEnvironment.CreateVhdx(allowKeepDebug, length);
            return testEnvironment;
        }

        public override void Dispose()
        {
            FileSystem.Dispose();
            base.Dispose();
        }

        private void CreateVhdx(bool allowKeepDebug, long length)
        {
            var diskStream = CreateVhdxStream(allowKeepDebug);
            Disk = Disk.InitializeDynamic(diskStream, Ownership.Dispose, length, 128 << 20);
            var gpt = GuidPartitionTable.Initialize(Disk);
            gpt.Create(gpt.FirstUsableSector, gpt.LastUsableSector, GuidPartitionTypes.WindowsBasicData, 0, null);
            var volume = VolumeManager.GetPhysicalVolumes(Disk).First();
            uint bytesPerSector = (uint)(volume.PhysicalGeometry?.BytesPerSector ?? 512);
            var clusterCount = 1 << 25;
            var clusterSize = length / clusterCount;
            var clusterBits = (int)Math.Ceiling(Math.Log(clusterSize) / Math.Log(2));
            if (clusterBits > 18)
                clusterBits = 18;
            else if (clusterBits < 11)
                clusterBits = 11;
            FileSystem = ExFatEntryFilesystem.Format(volume.Open(),
                new ExFatFormatOptions { SectorsPerCluster = (1u << clusterBits) / bytesPerSector });
        }

        private Stream CreateVhdxStream(bool allowKeepDebug)
        {
            if (!allowKeepDebug)
                return new MemoryStream();
            VhdxPath = Path.Combine(Path.GetTempPath(), $"exFAT test (to be removed) {Guid.NewGuid():N}.vhdx");
            return File.Create(VhdxPath, 1 << 20);
        }
    }
}