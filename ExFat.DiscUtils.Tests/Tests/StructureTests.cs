namespace ExFat.DiscUtils.Tests
{
    using System.Linq;
    using Core;
    using Core.Entries;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StructureTests
    {
        [TestMethod]
        public void DirectoryEntries()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatPartition(testEnvironment.PartitionStream);
                using (var rootDirectory = fs.OpenDirectory(fs.RootDirectoryDataDescriptor))
                {
                    var entries = rootDirectory.GetEntries().ToArray();
                    Assert.IsTrue(entries.OfType<FileNameExtensionExFatDirectoryEntry>().Any(e => e.FileName.Value == DiskContent.LongContiguousFileName));
                }
            }
        }

        [TestMethod]
        public void ValidGroupedEntries()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatPartition(testEnvironment.PartitionStream);
                using (var rootDirectory = fs.OpenDirectory(fs.RootDirectoryDataDescriptor))
                {
                    var entries = rootDirectory.GetMetaEntries().ToArray();
                    Assert.IsTrue(entries.Any(e => e.ExtensionsFileName == DiskContent.LongContiguousFileName));
                }
            }
        }

        [TestMethod]
        public void CheckHashes()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatPartition(testEnvironment.PartitionStream);
                using (var rootDirectory = fs.OpenDirectory(fs.RootDirectoryDataDescriptor))
                {
                    foreach (var entry in rootDirectory.GetMetaEntries())
                    {
                        if (entry.Primary is FileExFatDirectoryEntry)
                        {
                            var hash = fs.GetNameHash(entry.ExtensionsFileName);
                            Assert.AreEqual(entry.SecondaryStreamExtension.NameHash.Value, hash);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void AllocationBitmapExists()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var partition = new ExFatPartition(testEnvironment.PartitionStream);
                var bitmap = partition.GetAllocationBitmap();
                Assert.IsTrue(bitmap[2]);
                var allocate1 = bitmap.FindUnallocated();
                Assert.IsFalse(bitmap[allocate1]);
                var allocate10 = bitmap.FindUnallocated(10);
                Assert.IsFalse(bitmap[allocate10]);
                Assert.IsFalse(bitmap[allocate10 + 1]);
                Assert.IsFalse(bitmap[allocate10 + 2]);
                Assert.IsFalse(bitmap[allocate10 + 3]);
                Assert.IsFalse(bitmap[allocate10 + 4]);
                Assert.IsFalse(bitmap[allocate10 + 5]);
                Assert.IsFalse(bitmap[allocate10 + 6]);
                Assert.IsFalse(bitmap[allocate10 + 7]);
                Assert.IsFalse(bitmap[allocate10 + 8]);
                Assert.IsFalse(bitmap[allocate10 + 9]);
            }
        }
    }
}
