// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Environment;
    using IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Partition;
    using Partition.Entries;

    [TestClass]
    [TestCategory("Partition")]
    public class PartitionReadTests
    {
        internal static void ReadFile(string fileName, Func<ulong, ulong> getValueAtOffset,
            ulong? overrideLength = null, bool forward = true, bool forceSeek = false)
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx())
            {
                var fs = new ExFatPartition(testEnvironment.PartitionStream);
                ReadFile(fs, fileName, getValueAtOffset, overrideLength, forward, forceSeek);
            }
        }

        internal static void ReadFile(ExFatPartition partition, string fileName, Func<ulong, ulong> getValueAtOffset,
            ulong? overrideLength = null, bool forward = true, bool forceSeek = false)
        {
            var fileEntry = partition.GetMetaEntries(partition.RootDirectoryDataDescriptor)
                .Single(e => e.ExtensionsFileName == fileName);
            var length = overrideLength ?? fileEntry.SecondaryStreamExtension.DataLength.Value;
            var contiguous =
                fileEntry.SecondaryStreamExtension.GeneralSecondaryFlags.Value.HasAny(ExFatGeneralSecondaryFlags
                    .NoFatChain);
            using (var stream = partition.OpenDataStream(
                new DataDescriptor(fileEntry.SecondaryStreamExtension.FirstCluster.Value, contiguous, length, length),
                FileAccess.Read))
            {
                var vb = new byte[sizeof(ulong)];
                var range = Enumerable.Range(0, (int) (length / sizeof(ulong))).Select(r => r * sizeof(ulong));
                if (!forward)
                {
                    range = range.Reverse();
                    forceSeek = true;
                }
                foreach (var offset in range)
                {
                    if (forceSeek)
                        stream.Seek(offset, SeekOrigin.Begin);
                    stream.Read(vb, 0, vb.Length);
                    var v = LittleEndian.ToUInt64(vb);
                    Assert.AreEqual(v, getValueAtOffset((ulong) offset));
                }
                if (forward)
                    Assert.AreEqual(0, stream.Read(vb, 0, vb.Length));
            }
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongContiguousFull()
        {
            ReadFile(DiskContent.LongContiguousFileName, DiskContent.GetLongContiguousFileNameOffsetValue);
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongContiguousFullBackwards()
        {
            ReadFile(DiskContent.LongContiguousFileName, DiskContent.GetLongContiguousFileNameOffsetValue,
                forward: false);
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongContiguousLimited()
        {
            var length = (DiskContent.LongFileSize / 3 * 2) & ~7ul;
            ReadFile(DiskContent.LongContiguousFileName, DiskContent.GetLongContiguousFileNameOffsetValue, length);
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongSparseFull()
        {
            ReadFile(DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue);
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongSparseFullBackwards()
        {
            ReadFile(DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue, forward: false);
        }

        [TestMethod]
        [TestCategory("Read")]
        public void ReadLongSparseLimited()
        {
            var length = (DiskContent.LongFileSize / 3 * 2) & ~7ul;
            ReadFile(DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue, length);
        }
    }
}