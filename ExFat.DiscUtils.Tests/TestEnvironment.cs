namespace ExFat.DiscUtils
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using global::DiscUtils;
    using global::DiscUtils.Streams;
    using global::DiscUtils.Vhdx;

    internal class TestEnvironment : IDisposable
    {
        private readonly string _vhdxPath;
        private readonly Disk _disk;

        public Stream PartitionStream { get; }

        public TestEnvironment(bool allowDebugKeep = false)
        {
            _vhdxPath = Path.Combine(Path.GetTempPath(), $"exFAT test (to be removed) {Guid.NewGuid():N}.vhdx");

            using (var gzStream = GetType().Assembly.GetManifestResourceStream(GetType(), "exFAT.vhdx.gz"))
            using (var gzipStream = new GZipStream(gzStream, CompressionMode.Decompress))
            {
                var fileOptions = FileOptions.DeleteOnClose;
#if DEBUG
                if (allowDebugKeep)
                    fileOptions &= ~FileOptions.DeleteOnClose;
#endif
                var vhdxStream = File.Create(_vhdxPath, 1 << 20, fileOptions);
                gzipStream.CopyTo(vhdxStream);

                _disk = new Disk(vhdxStream, Ownership.Dispose);
                var volume = VolumeManager.GetPhysicalVolumes(_disk)[1];
                PartitionStream = volume.Open();
            }
        }

        public void Dispose()
        {
            PartitionStream.Dispose();
            _disk.Dispose();
            // Should not
            if (File.Exists(_vhdxPath))
                File.Delete(_vhdxPath);
        }
    }
}