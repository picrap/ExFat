// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                FileOptions fileOptions = 0;
//                var fileOptions = FileOptions.DeleteOnClose;
//#if DEBUG
//                if (allowDebugKeep)
//                    fileOptions &= ~FileOptions.DeleteOnClose;
//#endif
                var vhdxStream = allowDebugKeep ? (Stream)File.Create(_vhdxPath, 1 << 20, fileOptions) : new MemoryStream();
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
            // a check when required
            if (File.Exists(_vhdxPath))
            {
                try
                {
                    if (IsElevated)
                    {
                        var t = CheckDisk();
                        if (!t.Item1)
                            Assert.Fail("VHDX filesystem is found corrupted by CHKDSK: " + t.Item2);
                    }
                    else
                        Assert.Inconclusive("Not elevated");
                }
                finally
                {
                    File.Delete(_vhdxPath);
                }
            }
        }

        private static bool IsElevated
        {
            get
            {
                var id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }
        }

        private Tuple<bool, string> CheckDisk()
        {
            var previousDrives = DriveInfo.GetDrives();
            RunDiskPart("attach", _vhdxPath);
            var newDrives = DriveInfo.GetDrives();
            var mountedDrive = newDrives.FirstOrDefault(d => previousDrives.All(p => p.Name != d.Name));
            bool success = true;
            string checkResult = null;
            if (mountedDrive != null)
            {
                var result = ProcessUtility.Run("chkdsk", mountedDrive.Name.TrimEnd('\\'));
                success = result.Item1 == 0;
                checkResult = result.Item2;
            }
            RunDiskPart("detach", _vhdxPath);
            return Tuple.Create(success, checkResult);
        }

        private static void RunDiskPart(string action, string vdiskPath)
        {
            var scriptPath = Path.GetTempFileName();
            using (var scriptStream = File.CreateText(scriptPath))
            {
                scriptStream.WriteLine($"select vdisk file=\"{vdiskPath}\"");
                scriptStream.WriteLine($"{action} vdisk");
            }
            ProcessUtility.Run("diskpart", $"/s {scriptPath}");
            File.Delete(scriptPath);
        }
    }
}