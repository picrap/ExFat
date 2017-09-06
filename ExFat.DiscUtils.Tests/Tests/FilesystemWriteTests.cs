// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.CodeDom;
    using System.IO;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("Filesystem")]
    public class FilesystemWriteTests
    {
        [TestMethod]
        [TestCategory("Write")]
        public void AppendTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                using (var s = filesystem.OpenFile(file, FileAccess.ReadWrite))
                {
                    s.Seek(0, SeekOrigin.End);
                    s.WriteByte(123);
                }
                Assert.AreEqual(file.Length, (long)DiskContent.LongFileSize + 1);
                using (var s2 = filesystem.OpenFile(file, FileAccess.Read))
                {
                    s2.Seek(-1, SeekOrigin.End);
                    Assert.AreEqual(123, s2.ReadByte());
                    Assert.AreEqual(-1, s2.ReadByte());
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateDirectoryTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
                {
                    var directoryName = "Orphaaaan 1";
                    var s1 = filesystem.CreateDirectory(filesystem.RootDirectory, directoryName);
                    var f1 = filesystem.FindChild(filesystem.RootDirectory, directoryName);
                    // do more checks?
                    Assert.IsNotNull(f1);
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateSubDirectoryTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
                {
                    var s1 = filesystem.CreateDirectory(filesystem.RootDirectory, "SubOne");
                    var n2 = "SubTwo";
                    var s2 = filesystem.CreateDirectory(s1, n2);
                    var f2 = filesystem.FindChild(s1, n2);
                    Assert.IsNotNull(f2);
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateFileTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
                {
                    using (var s = filesystem.CreateFile(filesystem.RootDirectory, "a.txt"))
                        s.WriteByte(65);

                    var f = filesystem.FindChild(filesystem.RootDirectory, "a.txt");
                    using (var s2 = filesystem.OpenFile(f, FileAccess.Read))
                    {
                        Assert.AreEqual(65, s2.ReadByte());
                        Assert.AreEqual(-1, s2.ReadByte());
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void DeleteFileTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
                {
                    var f = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                    filesystem.Delete(f);
                    Assert.IsNull(filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void DeleteDirectoryTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
                {
                    var f = filesystem.FindChild(filesystem.RootDirectory, DiskContent.EmptyRootFolderFileName);
                    filesystem.Delete(f);
                    Assert.IsNull(filesystem.FindChild(filesystem.RootDirectory, DiskContent.EmptyRootFolderFileName));
                }
            }
        }
    }
}