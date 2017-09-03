namespace ExFat.Generator
{
    using System;
    using System.IO;
    using Core;
    using DiscUtils;

    public static class Program
    {
        public static void Main(string[] args)
        {
            const string drive = "X:";

            // label
            var driveInfo = new DriveInfo(drive);
            driveInfo.VolumeLabel = DiskContent.VolumeLabel;

            // long contiguous file
            using (var fc = File.Create(Path.Combine(drive, DiskContent.LongContiguousFileName)))
            {
                for (ulong offset = 0; offset < DiskContent.LongFileSize; offset += sizeof(ulong))
                {
                    var b = LittleEndian.GetBytes(DiskContent.GetLongContiguousFileNameOffsetValue(offset));
                    fc.Write(b, 0, b.Length);
                }
            }

            // long sparse files
            const uint chunks = 1u << 10;
            for (ulong offsetBase = 0; offsetBase < DiskContent.LongFileSize; offsetBase += chunks)
            {
                using (var fs1 = File.OpenWrite(Path.Combine(drive, DiskContent.LongSparseFile1Name)))
                using (var fs2 = File.OpenWrite(Path.Combine(drive, DiskContent.LongSparseFile2Name)))
                {
                    fs1.Seek(0, SeekOrigin.End);
                    fs2.Seek(0, SeekOrigin.End);
                    for (ulong subOffset = 0; subOffset < chunks; subOffset += sizeof(ulong))
                    {
                        var offset = offsetBase + subOffset;
                        var b1 = LittleEndian.GetBytes(DiskContent.GetLongSparseFile1NameOffsetValue(offset));
                        fs1.Write(b1, 0, b1.Length);
                        var b2 = LittleEndian.GetBytes(DiskContent.GetLongSparseFile2NameOffsetValue(offset));
                        fs2.Write(b2, 0, b2.Length);
                    }
                }
            }

            // An empty folder
            Directory.CreateDirectory(Path.Combine(drive, DiskContent.EmptyRootFolderFileName));

            // A folder full of garbage
            var longDirectoryPath = Path.Combine(drive, DiskContent.LongFolderFileName);
            Directory.CreateDirectory(longDirectoryPath);
            for (int subFileIndex = 0; subFileIndex < DiskContent.LongFolderEntriesCount; subFileIndex++)
            {
                var path = Path.Combine(longDirectoryPath, Guid.NewGuid().ToString("N"));
                using (var t = File.CreateText(path))
                {
                    t.WriteLine(subFileIndex);
                }
            }
        }
    }
}
