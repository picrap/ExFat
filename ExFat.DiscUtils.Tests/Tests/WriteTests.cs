// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Partition;

    [TestClass]
    [TestCategory("Partition")]
    public class WriteTests
    {
        private static void AppendTest(string fileName, Func<ulong, ulong> getOffsetValue)
        {
            using (var testEnvironment = new TestEnvironment(true))
            {
                using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
                    AppendTest(partition, fileName, getOffsetValue);
            }
        }

        private static void AppendTest(ExFatPartition partition, string fileName, Func<ulong, ulong> getOffsetValue)
        {
            using (var rootDirectory = partition.OpenDirectory(partition.RootDirectoryDataDescriptor))
            {
                var fileEntry = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == fileName);
                var buffer = new Byte[8];
                var dataDescriptor = fileEntry.DataDescriptor;
                using (var overwrite = partition.OpenDataStream(dataDescriptor, FileAccess.ReadWrite))
                {
                    for (ulong offset = 0; offset < 2 * DiskContent.LongFileSize; offset += 8)
                    {
                        LittleEndian.GetBytes(getOffsetValue(offset), buffer);
                        overwrite.Write(buffer, 0, 8);
                    }
                }
                using (var read = partition.OpenDataStream(new DataDescriptor(dataDescriptor.FirstCluster, false, DiskContent.LongFileSize * 2), FileAccess.Read))
                {
                    for (ulong offset = 0; offset < 2 * DiskContent.LongFileSize; offset += 8)
                    {
                        var bytesRead = read.Read(buffer, 0, buffer.Length);
                        Assert.AreEqual(bytesRead, buffer.Length);
                        var readValue = LittleEndian.ToUInt64(buffer);
                        var expectedValue = getOffsetValue(offset);
                        Assert.AreEqual(expectedValue, readValue);
                    }
                    Assert.AreEqual(0, read.Read(buffer, 0, buffer.Length));
                }
            }
        }

        [TestMethod]
        [TestCategory("Append")]
        [TestCategory("Write")]
        public void AppendSparseTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
            {
                AppendTest(partition, DiskContent.LongSparseFile1Name, offset => offset / 7);
                // now check nothing was overwritten
                ReadTests.ReadFile(partition, DiskContent.LongSparseFile2Name, DiskContent.GetLongSparseFile2NameOffsetValue);
            }
        }

        [TestMethod]
        [TestCategory("Append")]
        [TestCategory("Write")]
        public void AppendContiguousTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
            {
                AppendTest(partition, DiskContent.LongContiguousFileName, offset => offset / 7);
                // now check nothing was overwritten
                ReadTests.ReadFile(partition, DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue);
                ReadTests.ReadFile(partition, DiskContent.LongSparseFile2Name, DiskContent.GetLongSparseFile2NameOffsetValue);
            }
        }

        [TestMethod]
        [TestCategory("Append")]
        [TestCategory("Write")]
        public void CreateStreamTest()
        {
            using (var testEnvironment = new TestEnvironment(true))
            using (var partition = new ExFatPartition(testEnvironment.PartitionStream))
            {
                DataDescriptor dataDescriptor = null;
                using (var stream = partition.CreateDataStream(d => dataDescriptor = d))
                {
                    stream.WriteByte(1);
                }
                using (var s2 = partition.OpenDataStream(dataDescriptor, FileAccess.Read))
                {
                    Assert.AreEqual(1, s2.ReadByte());
                    Assert.AreEqual(-1, s2.ReadByte());
                }
            }
        }
    }
}