// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.Linq;
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
            using (var testEnvironment = new TestEnvironment())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var entries = filesystem.EnumerateEntries(@"\").ToArray();
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongContiguousFileName}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongSparseFile1Name}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.EmptyRootFolderFileName}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongFolderFileName}"));
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadRootFolderFilesTest()
        {
            using (var testEnvironment = new TestEnvironment())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var entries = filesystem.EnumerateFiles(@"\").ToArray();
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongContiguousFileName}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongSparseFile1Name}"));
                Assert.IsFalse(entries.Contains($@"\{DiskContent.EmptyRootFolderFileName}"));
                Assert.IsFalse(entries.Contains($@"\{DiskContent.LongFolderFileName}"));
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadRootFolderDirectoriesTest()
        {
            using (var testEnvironment = new TestEnvironment())
            using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
            {
                var entries = filesystem.EnumerateDirectories(@"\").ToArray();
                Assert.IsFalse(entries.Contains($@"\{DiskContent.LongContiguousFileName}"));
                Assert.IsFalse(entries.Contains($@"\{DiskContent.LongSparseFile1Name}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.EmptyRootFolderFileName}"));
                Assert.IsTrue(entries.Contains($@"\{DiskContent.LongFolderFileName}"));
            }
        }
    }
}