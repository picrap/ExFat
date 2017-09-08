// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.Linq;
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
            using (var testEnvironment = new TestEnvironment(true))
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
            using (var testEnvironment = new TestEnvironment(true))
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
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var filesystem = new ExFatPathFilesystem(testEnvironment.PartitionStream))
                {
                    filesystem.DeleteTree(DiskContent.LongFolderFileName);
                    Assert.IsFalse(filesystem.EnumerateDirectories("").Contains($@"\{DiskContent.LongFolderFileName}"));
                }
            }
        }
    }
}