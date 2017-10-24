// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Environment
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using global::DiscUtils;
    using global::DiscUtils.Streams;
    using global::DiscUtils.Vhdx;

    internal class StreamTestEnvironment : TestEnvironment
    {
        public Stream PartitionStream { get; private set; }

        public static StreamTestEnvironment FromExistingVhdx(bool allowDebugKeep = false)
        {
            var testEnvironment = new StreamTestEnvironment();
            testEnvironment.ExtractVhdx(allowDebugKeep);
            return testEnvironment;
        }

        public override void Dispose()
        {
            PartitionStream.Dispose();
            base.Dispose();
        }

        private void ExtractVhdx(bool allowDebugKeep)
        {
            VhdxPath = Path.Combine(Path.GetTempPath(), $"exFAT test (to be removed) {Guid.NewGuid():N}.vhdx");

            using (var gzStream = GetType().Assembly.GetManifestResourceStream(GetType(), "exFAT.vhdx.gz"))
            using (var gzipStream = new GZipStream(gzStream, CompressionMode.Decompress))
            {
                FileOptions fileOptions = 0;
                //                var fileOptions = FileOptions.DeleteOnClose;
                //#if DEBUG
                //                if (allowDebugKeep)
                //                    fileOptions &= ~FileOptions.DeleteOnClose;
                //#endif
                var vhdxStream = allowDebugKeep
                    ? (Stream) File.Create(VhdxPath, 1 << 20, fileOptions)
                    : new MemoryStream();
                gzipStream.CopyTo(vhdxStream);

                Disk = new Disk(vhdxStream, Ownership.Dispose);
                var volume = VolumeManager.GetPhysicalVolumes(Disk)[1];
                PartitionStream = volume.Open();
            }
        }
    }
}