namespace ExFat.DiscUtils.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IntegrityTests
    {
        [TestMethod]
        public void ValidVolume()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                Assert.IsTrue(ExFatFileSystem.Detect(testEnvironment.PartitionStream));
            }
        }
    }
}