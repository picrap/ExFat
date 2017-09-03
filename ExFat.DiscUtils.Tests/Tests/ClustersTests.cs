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
        public void ReadLongFileClusters()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatPartition(testEnvironment.PartitionStream);
                using (var rootDirectory = fs.OpenDirectory(fs.RootDirectoryDataDescriptor))
                {
                    var oneM = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == DiskContent.LongSparseFile1Name);
                    var clusters = new List<long>();
                    for (long c = oneM.SecondaryStreamExtension.FirstCluster.Value;; c = fs.GetNextCluster(c))
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
}