// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Filesystem;
    using global::DiscUtils;
    using global::DiscUtils.Streams;
    using Partition;

    public class ExFatFileSystem : DiscFileSystem
    {
        public const string Name = "Microsoft exFAT";

        private readonly Stream _partitionStream;
        private readonly ExFatPathFilesystem _filesystem;

        public override string FriendlyName => Name;

        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the file system is read-only or read-write.
        /// </summary>
        public override bool CanWrite => _partitionStream.CanWrite;

        /// <inheritdoc />
        /// <summary>
        /// Size of the Filesystem in bytes
        /// </summary>
        public override long Size => _filesystem.TotalSpace;

        /// <inheritdoc />
        /// <summary>
        /// Used space of the Filesystem in bytes
        /// </summary>
        public override long UsedSpace => _filesystem.UsedSpace;

        /// <inheritdoc />
        /// <summary>
        /// Available space of the Filesystem in bytes
        /// </summary>
        public override long AvailableSpace => _filesystem.AvailableSpace;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.DiscUtils.ExFatFileSystem" /> class.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <exception cref="T:System.InvalidOperationException">Given stream is not exFAT volume</exception>
        public ExFatFileSystem(Stream partitionStream)
        {
            _filesystem = new ExFatPathFilesystem(partitionStream);
            var bootSector = ExFatPartition.ReadBootSector(partitionStream);
            if (!bootSector.IsValid)
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
            using (var reader = _filesystem.Open(sourceFile, FileMode.Open, FileAccess.Read))
            using (var writer = _filesystem.Open(destinationFile, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
                reader.CopyTo(writer);
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
            return GetEntries(path, searchPattern, searchOption)
                .Where(e => e.Attributes.HasAny(FileAttributes.Directory)).Select(e => e.Path).ToArray();
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            var entries = GetEntries(path, searchPattern, searchOption);
            return entries.Where(e => !e.Attributes.HasAny(FileAttributes.Directory)).Select(e => e.Path).ToArray();
        }

        public override string[] GetFileSystemEntries(string path)
        {
            return GetEntries(path, null, SearchOption.TopDirectoryOnly).Select(e => e.Path).ToArray();
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return GetEntries(path, searchPattern, SearchOption.TopDirectoryOnly).Select(e => e.Path).ToArray();
        }

        private static Regex ConvertWildcardsToRegEx(string pattern)
        {
            if (pattern == null)
                return null;

            //if (!pattern.Contains("."))
            //    pattern += ".";

            string query = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "[^.]") + "$";
            return new Regex(query, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private IEnumerable<ExFatEntryInformation> GetEntries(string path, string searchPattern, SearchOption searchOption)
        {
            var regex = ConvertWildcardsToRegEx(searchPattern);
            var entry = _filesystem.GetInformation(path);
            if (entry == null || !entry.Attributes.HasAny(FileAttributes.Directory))
                throw new DirectoryNotFoundException();
            return GetEntries(entry, searchOption == SearchOption.TopDirectoryOnly ? 1 : int.MaxValue).Where(e => IsMatch(regex, e));
        }

        private static bool IsMatch(Regex regex, ExFatEntryInformation e)
        {
            if (regex == null)
                return true;
            var fileName = Path.GetFileName(e.Path);
            return regex.IsMatch(fileName);
        }

        private readonly ExFatEntryInformation[] _noEntry = new ExFatEntryInformation[0];

        private IEnumerable<ExFatEntryInformation> GetEntries(ExFatEntryInformation entryInformation, int depth)
        {
            if (depth == 0 || !entryInformation.Attributes.HasAny(FileAttributes.Directory))
                return _noEntry;
            return _filesystem.EnumerateEntries(entryInformation.Path).SelectMany(e => new[] { e }.Concat(GetEntries(e, depth - 1)));
        }

        /// <summary>
        /// Moves a directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The directory to move.</param>
        /// <param name="destinationDirectoryName">The target directory name.</param>
        public override void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            _filesystem.Move(sourceDirectoryName, Path.GetDirectoryName(destinationDirectoryName), Path.GetFileName(destinationDirectoryName));
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