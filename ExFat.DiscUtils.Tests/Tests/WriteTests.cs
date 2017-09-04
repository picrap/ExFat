namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Core;
    using Core.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WriteTests
    {

        public static ulong GetOffsetValue(ulong offset) => offset / 7;

        [TestMethod]
        public void AppendSparseTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                var partition = new ExFatPartition(testEnvironment.PartitionStream);
                using (var rootDirectory = partition.OpenDirectory(partition.RootDirectoryDataDescriptor))
                {
                    var fileEntry = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == DiskContent.LongSparseFile1Name);
                    var buffer = new Byte[8];
                    var dataDescriptor = fileEntry.DataDescriptor;
                    using (var overwrite = partition.OpenDataStream(dataDescriptor, FileAccess.ReadWrite))
                    {
                        for (ulong offset = 0; offset < 2 * DiskContent.LongFileSize; offset += 8)
                        {
                            LittleEndian.GetBytes(GetOffsetValue(offset), buffer);
                            overwrite.Write(buffer, 0, 8);
                        }
                    }
                    using (var read = partition.OpenDataStream(new DataDescriptor(dataDescriptor.FirstCluster, dataDescriptor.Contiguous, DiskContent.LongFileSize * 2), FileAccess.Read))
                    {
                        for (ulong offset = 0; offset < 2 * DiskContent.LongFileSize; offset += 8)
                        {
                            var bytesRead = read.Read(buffer, 0, buffer.Length);
                            Assert.AreEqual(bytesRead, buffer.Length);
                            var readValue = LittleEndian.ToUInt64(buffer);
                            var expectedValue = GetOffsetValue(offset);
                            Assert.AreEqual(expectedValue, readValue);
                        }
                        Assert.AreEqual(0, read.Read(buffer, 0, buffer.Length));
                    }
                }
            }
        }
    }
}