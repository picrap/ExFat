// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Environment;
    using IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Partition;

    [TestClass]
    [TestCategory("Structure")]
    public class PartitionClustersTests
    {
        [TestMethod]
        [TestCategory("Structure")]
        public void ReadLongFileClusters()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
            {
                var oneM = partition.GetMetaEntries(partition.RootDirectoryDataDescriptor)
                    .Single(e => e.ExtensionsFileName == DiskContent.LongSparseFile1Name);
                var clusters = new List<Cluster>();
                for (Cluster c = oneM.SecondaryStreamExtension.FirstCluster.Value;; c = partition.GetNextCluster(c))
                {
                    if (c.IsLast)
                        break;
                    if (!c.IsData)
                        Assert.Fail("Found invalid cluster (o'brother, where art thou?)");
                    clusters.Add(c);
                }
            }
        }
    }
}