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
    }
}
