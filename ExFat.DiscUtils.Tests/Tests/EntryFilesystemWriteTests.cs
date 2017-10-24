// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Environment;
    using Filesystem;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("EntryFilesystem")]
    public class EntryFilesystemWriteTests
    {
        [TestMethod]
        [TestCategory("Write")]
        public void AppendTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
            {
                var file = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                using (var s = filesystem.OpenFile(file, FileAccess.ReadWrite))
                {
                    s.Seek(0, SeekOrigin.End);
                    s.WriteByte(123);
                }
                Assert.AreEqual(file.Length, (long)DiskContent.LongFileSize + 1);
                using (var s2 = filesystem.OpenFile(file, FileAccess.Read))
                {
                    s2.Seek(-1, SeekOrigin.End);
                    Assert.AreEqual(123, s2.ReadByte());
                    Assert.AreEqual(-1, s2.ReadByte());
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateDirectoryTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var directoryName = "Orphaaaan 1";
                    var s1 = filesystem.CreateDirectory(filesystem.RootDirectory, directoryName);
                    var f1 = filesystem.FindChild(filesystem.RootDirectory, directoryName);
                    // do more checks?
                    Assert.IsNotNull(f1);
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateSubDirectoryTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var s1 = filesystem.CreateDirectory(filesystem.RootDirectory, "SubOne");
                    var n2 = "SubTwo";
                    var s2 = filesystem.CreateDirectory(s1, n2);
                    var f2 = filesystem.FindChild(s1, n2);
                    Assert.IsNotNull(f2);
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    using (var s = filesystem.CreateFile(filesystem.RootDirectory, "a.txt"))
                        s.WriteByte(65);

                    var f = filesystem.FindChild(filesystem.RootDirectory, "a.txt");
                    using (var s2 = filesystem.OpenFile(f, FileAccess.Read))
                    {
                        Assert.AreEqual(65, s2.ReadByte());
                        Assert.AreEqual(-1, s2.ReadByte());
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateEmptyFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    using (var s = filesystem.CreateFile(filesystem.RootDirectory, "a.txt"))
                    {
                    }

                    var f = filesystem.FindChild(filesystem.RootDirectory, "a.txt");
                    using (var s2 = filesystem.OpenFile(f, FileAccess.Read))
                    {
                        Assert.AreEqual(-1, s2.ReadByte());
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void DeleteFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var f = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                    filesystem.Delete(f);
                    Assert.IsNull(filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void DeleteDirectoryTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var f = filesystem.FindChild(filesystem.RootDirectory, DiskContent.EmptyRootFolderFileName);
                    filesystem.Delete(f);
                    Assert.IsNull(filesystem.FindChild(filesystem.RootDirectory, DiskContent.EmptyRootFolderFileName));
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void TruncateFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var f = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                    using (var s = filesystem.OpenFile(f, FileAccess.ReadWrite))
                    {
                        s.SetLength(16);
                        s.Seek(0, SeekOrigin.Begin);
                        var b = new byte[8];
                        Assert.AreEqual(8, s.Read(b, 0, b.Length));
                        Assert.AreEqual(DiskContent.GetLongContiguousFileNameOffsetValue(0), LittleEndian.ToUInt64(b));
                        Assert.AreEqual(8, s.Read(b, 0, b.Length));
                        Assert.AreEqual(DiskContent.GetLongContiguousFileNameOffsetValue(8), LittleEndian.ToUInt64(b));
                        Assert.AreEqual(0, s.Read(b, 0, b.Length));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void LengthenFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    using (var s = filesystem.CreateFile(filesystem.RootDirectory, "newlong"))
                    {
                        var length = 200000;
                        s.SetLength(length);
                        s.Seek(-1, SeekOrigin.End);
                        s.WriteByte(12);
                        Assert.AreEqual(length, s.Length);
                        s.WriteByte(34);
                        Assert.AreEqual(length + 1, s.Length);
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void MoveFileTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var aFile = filesystem.FindChild(filesystem.RootDirectory, DiskContent.LongContiguousFileName);
                    var aFolder = filesystem.FindChild(filesystem.RootDirectory, DiskContent.EmptyRootFolderFileName);
                    filesystem.Move(aFile, aFolder, "noob");
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void RandomReadWriteTest()
        {
            using (var testEnvironment = StreamTestEnvironment.FromExistingVhdx(true))
            {
                using (var filesystem = new ExFatEntryFilesystem(testEnvironment.PartitionStream))
                {
                    var testFolder = filesystem.CreateDirectory(filesystem.RootDirectory, "inner looping test");
                    var random = new Random(0);
                    var fileIndex = 0;
                    var catalogCache = new List<string>();
                    for (int loop = 0; loop < 10; loop++)
                    {
                        // delete some
                        var existingFiles = filesystem.EnumerateFileSystemEntries(testFolder).ToArray();
                        for (int index = 0; index < existingFiles.Length; index++)
                        {
                            if (random.Next(0, 2) == 0)
                            {
                                filesystem.Delete(existingFiles[index]);
                                Assert.IsTrue(catalogCache.Remove(existingFiles[index].Name));
                            }
                        }
                        // add some
                        var newFilesCount = random.Next(100, 300);
                        for (int index = 0; index < newFilesCount; index++)
                        {
                            ++fileIndex;
                            var fileName = fileIndex.ToString();
                            using (var s = filesystem.CreateFile(testFolder, fileName))
                            {
                                var l = random.Next(0, 1000);
                                var b = BitConverter.GetBytes(fileIndex);
                                for (int i = 0; i < l; i++)
                                    s.Write(b, 0, b.Length);
                            }
                            catalogCache.Add(fileName);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void NewPartitionTest()
        {
            using (EntryFilesystemTestEnvironment.FromNewVhdx(true)) { }
        }

        [TestMethod]
        [TestCategory("Write")]
        public void CreateFolderOnNewPartitionTest()
        {
            using (var testEnvironment = EntryFilesystemTestEnvironment.FromNewVhdx(true))
            {
                var filesystem = testEnvironment.FileSystem;
                filesystem.CreateDirectory(filesystem.RootDirectory, "folder");
                var folder = filesystem.FindChild(filesystem.RootDirectory, "folder");
                Assert.IsNotNull(folder);
            }
        }
    }
}