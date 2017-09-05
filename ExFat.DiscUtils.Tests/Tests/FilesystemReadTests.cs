// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("Filesystem")]
    public class FilesystemReadTests
    {
        [TestMethod]
        [TestCategory("Read")]
        public void ReadFindFile()
        {
            using (var testEnvironment = new TestEnvironment())
            using (var filesystem = new ExFatFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                Assert.IsNotNull(file);
            }
        }
    }
}