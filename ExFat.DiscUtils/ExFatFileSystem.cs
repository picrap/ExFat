// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils
{
    using System;
    using System.IO;
    using System.Linq;
    using Filesystem;
    using global::DiscUtils;
    using global::DiscUtils.Streams;
    using Partition;

    public class ExFatFileSystem : DiscFileSystem
    {
        public const string Name = "Microsoft exFAT";

        private readonly Stream _partitionStream;
        private ExFatBootSector _bootSector;
        private readonly ExFatPathFilesystem _filesystem;

        public override string FriendlyName => Name;

        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the file system is read-only or read-write.
        /// </summary>
        public override bool CanWrite => _partitionStream.CanWrite;

        public override long Size => throw new NotImplementedException();
        public override long UsedSpace => throw new NotImplementedException();
        public override long AvailableSpace => throw new NotImplementedException();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.DiscUtils.ExFatFileSystem" /> class.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <exception cref="T:System.InvalidOperationException">Given stream is not exFAT volume</exception>
        public ExFatFileSystem(Stream partitionStream)
        {
            _filesystem = new ExFatPathFilesystem(partitionStream);
            _bootSector = ExFatPartition.ReadBootSector(partitionStream);
            if (!_bootSector.IsValid)
                throw new InvalidOperationException("Given stream is not exFAT volume");
            _partitionStream = partitionStream;
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        /// <param name="disposing">The value <c>true</c> if Disposing.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _filesystem.Dispose();
        }

        /// <summary>
        /// Detects the specified partition stream.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <returns></returns>
        public static bool Detect(Stream partitionStream)
        {
            var bootSector = ExFatPartition.ReadBootSector(partitionStream);
            return bootSector.IsValid;
        }

        public override void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public override void CreateDirectory(string path)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(string path)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override void DeleteFile(string path)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override bool DirectoryExists(string path)
        {
            // TODO
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
            return _filesystem.EnumerateEntries(path).ToArray();
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override void MoveFile(string sourceName, string destinationName, bool overwrite)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override FileAttributes GetAttributes(string path)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override void SetAttributes(string path, FileAttributes newValue)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            return _filesystem.GetCreationTimeUtc(path);
        }

        public override void SetCreationTimeUtc(string path, DateTime newTime)
        {
            _filesystem.SetCreationTimeUtc(path, newTime);
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            return _filesystem.GetLastAccessTimeUtc(path);
        }

        public override void SetLastAccessTimeUtc(string path, DateTime newTime)
        {
            _filesystem.SetLastAccessTimeUtc(path, newTime);
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return _filesystem.GetLastWriteTimeUtc(path);
        }

        public override void SetLastWriteTimeUtc(string path, DateTime newTime)
        {
            _filesystem.SetLastWriteTimeUtc(path, newTime);
        }

        public override long GetFileLength(string path)
        {
            throw new NotImplementedException();
        }
    }
}