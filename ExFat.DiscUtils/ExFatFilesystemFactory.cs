// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils
{
    using System.IO;
    using global::DiscUtils;
    using global::DiscUtils.Vfs;
    using FileSystemInfo = global::DiscUtils.FileSystemInfo;

    [VfsFileSystemFactory]
    // ReSharper disable once UnusedMember.Global
    public class ExFatFilesystemFactory : VfsFileSystemFactory
    {
        public override FileSystemInfo[] Detect(Stream stream, VolumeInfo volume)
        {
            if (ExFatFileSystem.Detect(stream))
                return new FileSystemInfo[] { new VfsFileSystemInfo("exFAT", ExFatFileSystem.Name, Open) };

            return new FileSystemInfo[0];
        }

        private static DiscFileSystem Open(Stream stream, VolumeInfo volumeInfo, FileSystemParameters parameters)
        {
            return new ExFatFileSystem(stream);
        }
    }
}