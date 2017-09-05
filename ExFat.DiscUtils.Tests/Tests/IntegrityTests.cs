// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("Partition")]
    public class IntegrityTests
    {
        [TestMethod]
        [TestCategory("Detection")]
        public void ValidVolume()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                Assert.IsTrue(ExFatFileSystem.Detect(testEnvironment.PartitionStream));
            }
        }
    }
}