// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Environment;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("PathFilesystem")]
    public class PathFilesystemWriteTests
    {
        private bool IsAlmostMoreRecentThan(DateTime test, DateTime reference)
        {
            var dt = test - reference;
            // for some unknown (and strange) reason, a strict comparison fails on AppVeyor.
            // I'd love to see how they manage the time
            return dt.TotalHours > -1;
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateDirectory()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    var now = DateTime.UtcNow;
                    var path = @"zzzz";
                    filesystem.CreateDirectory(path);
                    var d = filesystem.GetCreationTimeUtc(path);
                    Assert.IsTrue(IsAlmostMoreRecentThan(d, now));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateDirectoryTree()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    var now = DateTime.UtcNow;
                    var path = @"a\b\c";
                    filesystem.CreateDirectory(path);
                    var d = filesystem.GetCreationTimeUtc(path);
                    Assert.IsTrue(IsAlmostMoreRecentThan(d, now));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void DeleteTree()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    filesystem.DeleteTree(DiskContent.LongFolderFileName);
                    Assert.IsFalse(filesystem.EnumerateEntries("")
                        .Any(e => e.Path == $@"\{DiskContent.LongFolderFileName}"));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateFileTree()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    filesystem.CreateDirectory("a");
                    using (var s = filesystem.Open(@"a\b.txt", FileMode.Create, FileAccess.ReadWrite))
                        s.WriteByte(66);
                    using (var r = filesystem.Open(@"a\b.txt", FileMode.Open, FileAccess.Read))
                    {
                        Assert.AreEqual(66, r.ReadByte());
                        Assert.AreEqual(-1, r.ReadByte());
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void MoveTree()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    filesystem.Move(DiskContent.LongSparseFile1Name, DiskContent.EmptyRootFolderFileName);
                    Assert.IsNull(filesystem.GetInformation(DiskContent.LongSparseFile1Name));
                    Assert.IsNotNull(
                        filesystem.GetInformation(DiskContent.EmptyRootFolderFileName + "\\" +
                                                  DiskContent.LongSparseFile1Name));
                }
            }
        }
    }
}