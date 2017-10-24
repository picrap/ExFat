// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.Linq;
    using Environment;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("PathFilesystem")]
    public class PathFilesystemReadTests
    {
        [TestMethod]
        [TestCategory("Read")]
        public void ReadRootFolderEntriesTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var entries = filesystem.EnumerateEntries(@"\").ToArray();
                Assert.IsTrue(entries.Any(e => e.Path == DiskContent.LongContiguousFileName));
                Assert.IsTrue(entries.Any(e => e.Path == DiskContent.LongSparseFile1Name));
                Assert.IsTrue(entries.Any(e => e.Path == DiskContent.EmptyRootFolderFileName));
                Assert.IsTrue(entries.Any(e => e.Path == DiskContent.LongFolderFileName));
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadSubFolderFilesTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var entries = filesystem.EnumerateEntries(DiskContent.LongFolderFileName).ToArray();
                Assert.AreEqual(DiskContent.LongFolderEntriesCount, entries.Length);
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadDatesTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var c = filesystem.GetCreationTime(DiskContent.LongContiguousFileName);
            }
        }
    }
}