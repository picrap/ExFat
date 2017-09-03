namespace ExFat.DiscUtils
{
    using System;
    using System.IO;
    using Core;
    using global::DiscUtils;
    using global::DiscUtils.Streams;

    public class ExFatFileSystem : DiscFileSystem
    {
        private Stream _partitionStream;
        private ExFatPartition _partition;
        private ExFatBootSector _bootSector;

        public override string FriendlyName => "Microsoft exFAT";

        /// <summary>
        /// Gets a value indicating whether the file system is read-only or read-write.
        /// </summary>
        public override bool CanWrite => _partitionStream.CanWrite;
        public override long Size { get; }
        public override long UsedSpace { get; }
        public override long AvailableSpace { get; }

        public ExFatFileSystem(Stream partitionStream)
        {
            _partition = new ExFatPartition(partitionStream);
            _bootSector = ExFatPartition.ReadBootSector(partitionStream);
            if (!_bootSector.IsValid)
                throw new InvalidOperationException("Given stream is not exFAT volume");
            _partitionStream = partitionStream;
        }

        public static bool Detect(Stream partitionStream)
        {
            var fs = new ExFatPartition(partitionStream);
            var bootSector = ExFatPartition.ReadBootSector(partitionStream);
            return bootSector.IsValid;
        }

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