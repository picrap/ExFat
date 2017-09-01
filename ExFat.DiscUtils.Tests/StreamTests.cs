namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.Linq;
    using Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamTests
    {
        [TestMethod]
        public void Read1MDirect()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFilesystemAccessor(testEnvironment.PartitionStream);
                var rootDirectoryStream = fs.OpenClusters(fs.BootSector.RootDirectory.Value);
                var rootDirectory = new ExFatDirectory(rootDirectoryStream);

                var oneM = rootDirectory.GetGroupedEntries().Single(e => e.ExtensionsFileName == "1M");
                using (var stream = fs.OpenClusters(oneM.SecondaryStreamExtension.FirstCluster.Value, oneM.SecondaryStreamExtension.DataLength.Value))
                {
                    var vb = new byte[4];
                    for (uint index = 0; index < 1 << 20; index += 4)
                    {
                        if (index == 512 * 256 - 8)
                        {
                        }
                        stream.Read(vb, 0, vb.Length);
                        var v = LittleEndian.ToUInt32(vb);
                        Assert.AreEqual(v, index);
                    }
                    Assert.AreEqual(0, stream.Read(vb, 0, vb.Length));
                }
            }
        }
    }
}
