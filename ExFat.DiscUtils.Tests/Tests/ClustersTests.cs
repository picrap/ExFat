// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Partition;

    [TestClass]
    [TestCategory("Structure")]
    public class ClustersTests
    {
        [TestMethod]
        [TestCategory("Structure")]
        public void ReadLongFileClusters()
        {
            using (var testEnvironment = new TestEnvironment())
            using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
            using (var rootDirectory = partition.OpenDirectory(partition.RootDirectoryDataDescriptor))
            {
                var oneM = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == DiskContent.LongSparseFile1Name);
                var clusters = new List<long>();
                for (long c = oneM.SecondaryStreamExtension.FirstCluster.Value; ; c = partition.GetNextCluster(c))
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
