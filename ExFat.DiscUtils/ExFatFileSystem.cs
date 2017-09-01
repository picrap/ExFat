namespace ExFat.DiscUtils
{
    using System;
    using System.IO;
    using global::DiscUtils;
    using global::DiscUtils.Streams;

    public class ExFatFileSystem: DiscFileSystem
    {
        public override string FriendlyName { get; }
        public override bool CanWrite { get; }
        public override long Size { get; }
        public override long UsedSpace { get; }
        public override long AvailableSpace { get; }

        public override void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public override void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public override bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public override bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            throw new NotImplementedException();
        }

        public override void MoveFile(string sourceName, string destinationName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public override FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        public override void SetAttributes(string path, FileAttributes newValue)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public override void SetCreationTimeUtc(string path, DateTime newTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public override void SetLastAccessTimeUtc(string path, DateTime newTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public override void SetLastWriteTimeUtc(string path, DateTime newTime)
        {
            throw new NotImplementedException();
        }

        public override long GetFileLength(string path)
        {
            throw new NotImplementedException();
        }
    }
}