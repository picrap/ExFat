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
    }
}