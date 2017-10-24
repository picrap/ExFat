// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.IO;
    using System.Linq;
    using Environment;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("DiscUtils")]
    public class DiscFilesystemTests
    {
        [TestMethod]
        [TestCategory("Read")]
        public void ReadAllFiles()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
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
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
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
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
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

        [TestMethod]
        [TestCategory("Write")]
        public void MoveFile()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            {
                using (var filesystem = new ExFatFileSystem(testEnvironment.PartitionStream))
                {
                    using (var a = filesystem.OpenFile("a", FileMode.Create))
                        a.WriteByte(1);
                    Assert.IsTrue(filesystem.FileExists("a"));
                    filesystem.MoveFile("a", "b");
                    Assert.IsFalse(filesystem.FileExists("a"));
                    Assert.IsTrue(filesystem.FileExists("b"));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void MoveFileToDirectory()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            {
                using (var filesystem = new ExFatFileSystem(testEnvironment.PartitionStream))
                {
                    using (var a = filesystem.OpenFile("a", FileMode.Create))
                        a.WriteByte(1);
                    filesystem.CreateDirectory("d");
                    Assert.IsTrue(filesystem.FileExists("a"));
                    filesystem.MoveFile("a", "d");
                    Assert.IsFalse(filesystem.FileExists("a"));
                    Assert.IsTrue(filesystem.FileExists("d\\a"));
                }
            }
        }
    }
}