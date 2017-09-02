namespace ExFat.DiscUtils.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClustersTests
    {
        [TestMethod]
        public void Read1MClusters()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFilesystemAccessor(testEnvironment.PartitionStream);
                var rootDirectoryStream = fs.OpenClusters(fs.BootSector.RootDirectory.Value, false);
                var rootDirectory = new ExFatDirectory(rootDirectoryStream);

                var oneM = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == DiskContent.LongSparseFile1Name);
                var clusters = new List<long>();
                for (long c = oneM.SecondaryStreamExtension.FirstCluster.Value; ; c = fs.GetNextCluster(c))
                {
                    if (c < 0)
                        break;
                    if (c < 2)
                        Assert.Fail("Found invalid cluster (o'brother, where art thou?)");
                    clusters.Add(c);
                }
            }
        }
    }
}