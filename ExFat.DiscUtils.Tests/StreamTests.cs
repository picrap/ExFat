namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.Linq;
    using Core;
    using Core.Entries;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamTests
    {
        private void ReadFile(string fileName, Func<ulong, ulong> getValueAtOffset, ulong? overrideLength)
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFilesystemAccessor(testEnvironment.PartitionStream);
                var rootDirectoryStream = fs.OpenClusters(fs.BootSector.RootDirectory.Value, false);
                var rootDirectory = new ExFatDirectory(rootDirectoryStream);

                var fileEntry = rootDirectory.GetMetaEntries().Single(e => e.ExtensionsFileName == fileName);
                var length = overrideLength ?? fileEntry.SecondaryStreamExtension.DataLength.Value;
                var contiguous = fileEntry.SecondaryStreamExtension.GeneralSecondaryFlags.Value.HasFlag(ExFatGeneralSecondaryFlags.NoFatChain);
                using (var stream = fs.OpenClusters(fileEntry.SecondaryStreamExtension.FirstCluster.Value, contiguous, length))
                {
                    var vb = new byte[sizeof(ulong)];
                    for (ulong offset = 0; offset < length; offset += (uint)vb.Length)
                    {
                        if (offset == 512 * 256 - 8)
                        {
                        }
                        stream.Read(vb, 0, vb.Length);
                        var v = LittleEndian.ToUInt64(vb);
                        Assert.AreEqual(v, getValueAtOffset(offset));
                    }
                    Assert.AreEqual(0, stream.Read(vb, 0, vb.Length));
                }
            }
        }

        [TestMethod]
        public void ReadLongContiguousFull()
        {
            ReadFile(DiskContent.LongContiguousFileName, DiskContent.GetLongContiguousFileNameOffsetValue, null);
        }

        [TestMethod]
        public void ReadLongContiguousLimited()
        {
            var length = (DiskContent.LongFileSize / 3 * 2) & ~8ul;
            ReadFile(DiskContent.LongContiguousFileName, DiskContent.GetLongContiguousFileNameOffsetValue, length);
        }

        [TestMethod]
        public void ReadLongSparseFull()
        {
            ReadFile(DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue, null);
        }

        [TestMethod]
        public void ReadLongSparseLimited()
        {
            var length = (DiskContent.LongFileSize / 3 * 2) & ~8ul;
            ReadFile(DiskContent.LongSparseFile1Name, DiskContent.GetLongSparseFile1NameOffsetValue, length);
        }
    }
}
