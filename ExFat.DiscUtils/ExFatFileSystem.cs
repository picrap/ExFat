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
            _filesystem.CreateDirectory(path);
        }

        public override void DeleteDirectory(string path)
        {
            _filesystem.DeleteTree(path);
        }

        public override void DeleteFile(string path)
        {
            _filesystem.Delete(path);
        }

        public override bool DirectoryExists(string path)
        {
            var information = _filesystem.GetInformation(path);
            return information != null && information.Attributes.HasAny(FileAttributes.Directory);
        }

        public override bool FileExists(string path)
        {
            var information = _filesystem.GetInformation(path);
            return information != null && !information.Attributes.HasAny(FileAttributes.Directory);
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

        /// <inheritdoc />
        /// <summary>
        /// Moves a file, allowing an existing file to be overwritten.
        /// </summary>
        /// <param name="sourceName">The file to move.</param>
        /// <param name="destinationName">The target file name.</param>
        /// <param name="overwrite">Whether to permit a destination file to be overwritten.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        public override void MoveFile(string sourceName, string destinationName, bool overwrite)
        {
            var information = _filesystem.GetInformation(destinationName);
            string targetDirectory, targetName;
            if (information != null)
            {
                if (information.Attributes.HasAny(FileAttributes.Directory))
                {
                    targetDirectory = destinationName;
                    targetName = null;
                }
                else
                {
                    if (overwrite)
                        DeleteFile(destinationName);
                    throw new IOException();
                }
            }
            else
            {
                targetDirectory = Path.GetDirectoryName(destinationName);
                targetName = Path.GetFileName(destinationName);
            }
            _filesystem.Move(sourceName, targetDirectory, targetName);
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            return SparseStream.FromStream(_filesystem.Open(path, mode, access), Ownership.Dispose);
        }

        public override FileAttributes GetAttributes(string path)
        {
            var information = _filesystem.GetInformation(path);
            if (information == null)
                throw new FileNotFoundException();
            return information.Attributes;
        }

        public override void SetAttributes(string path, FileAttributes newValue)
        {
            var information = _filesystem.GetInformation(path);
            if (information == null)
                throw new FileNotFoundException();
            information.Attributes = newValue;
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
            var information = _filesystem.GetInformation(path);
            if (information == null)
                throw new FileNotFoundException();
            return information.Length;
        }
    }
}