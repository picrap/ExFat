// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Environment
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using global::DiscUtils.Vhdx;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class TestEnvironment : IDisposable
    {
        protected string VhdxPath;
        protected Disk Disk;

        protected TestEnvironment()
        {
        }

        private static bool IsElevated
        {
            get
            {
                var id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }
        }

        public virtual void Dispose()
        {
            Disk?.Dispose();
            // a check when required
            if (VhdxPath != null && File.Exists(VhdxPath))
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
                    File.Delete(VhdxPath);
                }
            }
        }

        private Tuple<bool, string> CheckDisk()
        {
            var previousDrives = DriveInfo.GetDrives();
            RunDiskPart("attach", VhdxPath);
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
            RunDiskPart("detach", VhdxPath);
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