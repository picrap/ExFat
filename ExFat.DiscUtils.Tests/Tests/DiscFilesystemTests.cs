// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("DiscUtils")]
    public class DiscFilesystemTests
    {
        [TestMethod]
        [TestCategory("Read")]
        public void ReadAllFiles()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                using (var filesystem = new ExFatFileSystem(testEnvironment.PartitionStream))
                {
                    var allFiles = filesystem.GetFiles("", "0*", SearchOption.AllDirectories);
                    Assert.IsTrue(allFiles.All(p => Path.GetFileName(p).StartsWith("0")));
                }
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadRootFiles()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                using (var filesystem = new ExFatFileSystem(testEnvironment.PartitionStream))
                {
                    var allFiles = filesystem.GetFiles("");
                    Assert.IsTrue(allFiles.Contains(DiskContent.LongContiguousFileName));
                    Assert.IsTrue(allFiles.Contains(DiskContent.LongSparseFile1Name));
                    Assert.IsTrue(allFiles.Contains(DiskContent.LongSparseFile2Name));
                    Assert.IsFalse(allFiles.Contains(DiskContent.EmptyRootFolderFileName));
                    Assert.IsFalse(allFiles.Contains(DiskContent.LongFolderFileName));
                }
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadRootDirectories()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                using (var filesystem = new ExFatFileSystem(testEnvironment.PartitionStream))
                {
                    var allDirectories = filesystem.GetDirectories("");
                    Assert.IsFalse(allDirectories.Contains(DiskContent.LongContiguousFileName));
                    Assert.IsFalse(allDirectories.Contains(DiskContent.LongSparseFile1Name));
                    Assert.IsFalse(allDirectories.Contains(DiskContent.LongSparseFile2Name));
                    Assert.IsTrue(allDirectories.Contains(DiskContent.EmptyRootFolderFileName));
                    Assert.IsTrue(allDirectories.Contains(DiskContent.LongFolderFileName));
                }
            }
        }
    }
}