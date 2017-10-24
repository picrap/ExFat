// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.IO;
    using Environment;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("EntryFilesystem")]
    public class EntryFilesystemReadTests
    {
        [TestMethod]
        [TestCategory("Read")]
        public void FindFile()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                Assert.IsNotNull(file);
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadFile()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                var vb = new byte[sizeof(ulong)];
                using (var stream = filesystem.OpenFile(file, FileAccess.Read))
                {
                    for (ulong offset = 0; offset < DiskContent.LongFileSize; offset += 8)
                    {
                        stream.Read(vb, 0, vb.Length);
                        var v = LittleEndian.ToUInt64(vb);
                        Assert.AreEqual(v, DiskContent.GetLongContiguousFileNameOffsetValue(offset));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void UpdateLastAccessTime()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                var access0 = file.LastAccessTime;
                using (var stream = filesystem.OpenFile(file, FileAccess.Read))
                {
                }
                var access1 = file.LastAccessTime;
                Assert.IsTrue(access1 > access0);
                var file2 = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                var access2 = file2.LastAccessTime;
                Assert.AreEqual(access1, access2);
            }
        }
    }
}