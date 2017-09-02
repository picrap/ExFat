namespace ExFat.DiscUtils.Tests
{
    using System.Linq;
    using Core;
    using Core.Entries;
    using Core.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StructureTests
    {
        [TestMethod]
        public void DirectoryEntries()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFilesystemAccessor(testEnvironment.PartitionStream);
                var rootDirectoryStream = fs.OpenClusters(fs.BootSector.RootDirectory.Value);
                var rootDirectory = new ExFatDirectory(rootDirectoryStream);

                var entries = rootDirectory.GetEntries().ToArray();
                Assert.IsTrue(entries.OfType<FileNameExtensionExFatDirectoryEntry>().Any(e => e.FileName.Value == "1M"));
            }
        }

        [TestMethod]
        public void ValidGroupedEntries()
        {
            using (var testEnvironment = new TestEnvironment())
            {
                var fs = new ExFatFilesystemAccessor(testEnvironment.PartitionStream);
                var rootDirectoryStream = fs.OpenClusters(fs.BootSector.RootDirectory.Value);
                var rootDirectory = new ExFatDirectory(rootDirectoryStream);

                var entries = rootDirectory.GetGroupedEntries().ToArray();
                Assert.IsTrue(entries.Any(e => e.ExtensionsFileName == "1M"));
            }
        }
    }
}
