namespace ExFat.DiscUtils.Tests
{
    using Core;
    using Core.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StructureTests
    {
        [TestMethod]
        public void ValidVolume()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFS(testEnvironment.PartitionStream);
                var rootDirectoryStream = new ClusterStream(fs, fs, fs.BootSector.RootDirectory.Value, null);
                var d = new ExFatDirectory(rootDirectoryStream);
            }
        }
    }
}
