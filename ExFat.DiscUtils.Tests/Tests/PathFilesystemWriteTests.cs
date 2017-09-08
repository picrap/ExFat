// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("PathFilesystem")]
    public class PathFilesystemWriteTests
    {
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
                    Assert.IsTrue(d >= now);
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
                    Assert.IsTrue(d >= now);
                }
            }
        }
    }
}