// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Partition;

    /// <inheritdoc />
    /// <summary>
    /// High-level file system, which works with paths
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class ExFatPathFilesystem : IDisposable
    {
        private readonly ExFatEntryFilesystem _entryFilesystem;
        private readonly Cache<string, ExFatFilesystemEntry> _entries;
        private readonly object _entriesLock = new object();
        private readonly char[] _separators;
        /// <summary>
        /// The default separators
        /// </summary>
        public static readonly char[] DefaultSeparators = new[] { '\\', '/' };

        /// <summary>
        /// Gets the total size.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        public long TotalSpace => _entryFilesystem.TotalSpace;

        /// <summary>
        /// Gets the used space
        /// </summary>
        /// <value>
        /// The used space.
        /// </value>
        public long UsedSpace => _entryFilesystem.UsedSpace;

        /// <summary>
        /// Gets the available size.
        /// </summary>
        /// <value>
        /// The available size.
        /// </value>
        public long AvailableSpace => _entryFilesystem.AvailableSpace;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.Filesystem.ExFatPathFilesystem" /> class.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="pathSeparators">The path separators.</param>
        public ExFatPathFilesystem(Stream partitionStream, ExFatOptions flags = ExFatOptions.Default, char[] pathSeparators = null)
            : this(new ExFatPartition(partitionStream), flags, pathSeparators)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.Filesystem.ExFatPathFilesystem" /> class.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="pathSeparators">The path separators.</param>
        /// <inheritdoc />
        public ExFatPathFilesystem(ExFatPartition partition, ExFatOptions flags = ExFatOptions.Default, char[] pathSeparators = null)
            : this(new ExFatEntryFilesystem(partition, flags), pathSeparators)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatPathFilesystem" /> class.
        /// </summary>
        /// <param name="entryFilesystem">The entry filesystem.</param>
        /// <param name="pathSeparators">The path separators.</param>
        public ExFatPathFilesystem(ExFatEntryFilesystem entryFilesystem, char[] pathSeparators = null)
        {
            _entryFilesystem = entryFilesystem;
            _entries = new Cache<string, ExFatFilesystemEntry>(Environment.ProcessorCount * (8 + 100));
            _separators = pathSeparators ?? DefaultSeparators;
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _entryFilesystem.Dispose();
        }

        private ExFatFilesystemEntry GetEntry(string path)
        {
            if (path == null)
                throw new ArgumentNullException();
            path = CleanupPath(path);
            if (path == "")
                return _entryFilesystem.RootDirectory;

            lock (_entriesLock)
            {
                if (_entries.TryGetValue(path, out var entry))
                    return entry;

                var pn = GetParentAndName(path);
                entry = GetEntry(pn.Item1, pn.Item2);
                return Register(entry, path);
            }
        }

        private ExFatFilesystemEntry Register(ExFatFilesystemEntry entry, string cleanPath)
        {
            lock (_entriesLock)
                _entries[cleanPath] = entry;
            return entry;
        }

        private Tuple<string, string> GetParentAndName(string cleanPath)
        {
            var separatorIndex = cleanPath.LastIndexOfAny(_separators);
            if (separatorIndex < 0)
                return Tuple.Create("", cleanPath);
            return Tuple.Create(cleanPath.Substring(0, separatorIndex), cleanPath.Substring(separatorIndex + 1));
        }

        private string CleanupPath(string path)
        {
            return path.Trim(_separators);
        }

        private ExFatFilesystemEntry GetEntry(string cleanParentPath, string childName)
        {
            var parentEntry = GetEntry(cleanParentPath);
            if (parentEntry == null)
                return null;
            return _entryFilesystem.FindChild(parentEntry, childName);
        }

        private string GetPath(string cleanParentPath, ExFatFilesystemEntry entry)
        {
            var fileName = entry.MetaEntry.ExtensionsFileName;
            // on root entries, the direct name is returned
            if (cleanParentPath == "")
                return fileName;
            return $"{cleanParentPath}{_separators[0]}{fileName}";
        }

        private ExFatFilesystemEntry GetSafeDirectory(string cleanDirectoryPath)
        {
            var directory = GetEntry(cleanDirectoryPath);
            if (directory == null)
                throw new DirectoryNotFoundException();
            if (!directory.IsDirectory)
                throw new IOException();
            return directory;
        }

        private ExFatFilesystemEntry GetSafeEntry(string cleanPath)
        {
            var entry = GetEntry(cleanPath);
            if (entry == null)
                throw new FileNotFoundException();
            return entry;
        }

        private void UpdateSafeEntry(string cleanPath, Action<ExFatFilesystemEntry> update)
        {
            var entry = GetSafeEntry(cleanPath);
            update(entry);
            _entryFilesystem.Update(entry);
        }

        /// <summary>
        /// Enumerates the entries.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IEnumerable<ExFatEntryInformation> EnumerateEntries(string directoryPath)
        {
            var cleanDirectoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(cleanDirectoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Select(e => new ExFatEntryInformation(_entryFilesystem, e, GetPath(cleanDirectoryPath, e)));
        }

        /// <summary>
        /// Gets the creation time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetCreationTime(string path) => GetSafeEntry(path).CreationTime;
        /// <summary>
        /// Gets the creation time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetCreationTimeUtc(string path) => GetSafeEntry(path).CreationTimeUtc;
        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastWriteTime(string path) => GetSafeEntry(path).LastWriteTime;
        /// <summary>
        /// Gets the last write time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastWriteTimeUtc(string path) => GetSafeEntry(path).LastWriteTimeUtc;
        /// <summary>
        /// Gets the last access time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastAccessTime(string path) => GetSafeEntry(path).LastAccessTime;
        /// <summary>
        /// Gets the last access time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastAccessTimeUtc(string path) => GetSafeEntry(path).LastAccessTimeUtc;

        /// <summary>
        /// Sets the creation time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="creationTime">The creation time.</param>
        public void SetCreationTime(string path, DateTime creationTime) => UpdateSafeEntry(path, e => e.CreationDateTimeOffset = creationTime.ToLocalTime());
        /// <summary>
        /// Sets the creation time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="creationTime">The creation time.</param>
        public void SetCreationTimeUtc(string path, DateTime creationTime) => UpdateSafeEntry(path, e => e.CreationDateTimeOffset = creationTime.ToUniversalTime());
        /// <summary>
        /// Sets the last write time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lastWriteTime">The last write time.</param>
        public void SetLastWriteTime(string path, DateTime lastWriteTime) => UpdateSafeEntry(path, e => e.LastWriteDateTimeOffset = lastWriteTime.ToLocalTime());
        /// <summary>
        /// Sets the last write time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lastWriteTime">The last write time.</param>
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTime) => UpdateSafeEntry(path, e => e.LastWriteDateTimeOffset = lastWriteTime.ToUniversalTime());
        /// <summary>
        /// Sets the last access time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lastAccessTime">The last access time.</param>
        public void SetLastAccessTime(string path, DateTime lastAccessTime) => UpdateSafeEntry(path, e => e.LastAccessDateTimeOffset = lastAccessTime.ToLocalTime());
        /// <summary>
        /// Sets the last access time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lastAccessTime">The last access time.</param>
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTime) => UpdateSafeEntry(path, e => e.LastAccessDateTimeOffset = lastAccessTime.ToUniversalTime());

        private ExFatFilesystemEntry CreateDirectoryEntry(string cleanPath)
        {
            var existingDirectory = GetEntry(cleanPath);
            if (existingDirectory != null)
                return existingDirectory;
            var pn = GetParentAndName(cleanPath);
            var parentDirectory = CreateDirectoryEntry(pn.Item1);
            var directoryEntry = _entryFilesystem.CreateDirectory(parentDirectory, pn.Item2);
            return Register(directoryEntry, cleanPath);
        }

        /// <summary>
        /// Creates a directory under the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CreateDirectory(string path)
        {
            CreateDirectoryEntry(CleanupPath(path));
        }

        /// <summary>
        /// Deletes the specified entry at given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.IOException"></exception>
        public void Delete(string path)
        {
            var cleanPath = CleanupPath(path);
            var entry = GetSafeEntry(cleanPath);
            if (entry.IsDirectory)
            {
                if (_entryFilesystem.EnumerateFileSystemEntries(entry).Any())
                    throw new IOException();
            }
            _entryFilesystem.Delete(entry);
            Register(null, cleanPath);
        }

        /// <summary>
        /// Deletes the tree at given path.
        /// If the entry is a file, simply deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void DeleteTree(string path)
        {
            var cleanPath = CleanupPath(path);
            var entry = GetSafeEntry(cleanPath);
            if (entry.IsDirectory)
            {
                foreach (var childPath in EnumerateEntries(cleanPath))
                    DeleteTree(childPath.Path);
            }
            Delete(cleanPath);
        }

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public ExFatEntryInformation GetInformation(string path)
        {
            var cleanPath = CleanupPath(path);
            var entry = GetEntry(cleanPath);
            if (entry == null)
                return null;
            return new ExFatEntryInformation(_entryFilesystem, entry, cleanPath);
        }

        /// <summary>
        /// Opens or creates a file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="access">The access.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.IO.IOException">
        /// </exception>
        public Stream Open(string path, FileMode mode, FileAccess access)
        {
            var cleanPath = CleanupPath(path);
            var parentAndName = GetParentAndName(cleanPath);
            var parentEntry = GetEntry(parentAndName.Item1);
            if (parentEntry == null)
                throw new DirectoryNotFoundException();
            var child = _entryFilesystem.FindChild(parentEntry, parentAndName.Item2);
            // not existing?
            if (child == null)
            {
                if (mode == FileMode.Append || mode == FileMode.Open || mode == FileMode.Truncate)
                    throw new FileNotFoundException();
                return _entryFilesystem.CreateFile(parentEntry, parentAndName.Item2);
            }

            if (child.IsDirectory)
                throw new IOException();
            if (mode == FileMode.CreateNew)
                throw new IOException();
            var stream = _entryFilesystem.OpenFile(child, access);
            if (mode == FileMode.Truncate)
                stream.SetLength(0);
            if (mode == FileMode.Append)
                stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        /// <summary>
        /// Moves the specified source.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetDirectory">The target directory. null to stay in same directory</param>
        /// <param name="targetName">Name of the target. null to keep original name</param>
        public void Move(string sourcePath, string targetDirectory, string targetName = null)
        {
            if (targetDirectory == null && targetName == null)
                throw new ArgumentNullException(nameof(targetDirectory), "Either targetDirectory or targetName has to be provided");

            var cleanSourcePath = CleanupPath(sourcePath);
            if (targetDirectory == null)
                targetDirectory = GetParentAndName(cleanSourcePath).Item1;
            var cleanTargetDirectory = CleanupPath(targetDirectory);

            var sourceEntry = GetEntry(cleanSourcePath);
            if (sourceEntry == null)
                throw new FileNotFoundException();
            var targetDirectoryEntry = GetEntry(cleanTargetDirectory);
            if (targetDirectoryEntry == null)
                throw new FileNotFoundException();

            _entryFilesystem.Move(sourceEntry, targetDirectoryEntry, targetName);
            Register(null, cleanSourcePath);
        }

        /// <summary>
        /// Formats the specified partition stream.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <param name="options">The options.</param>
        /// <param name="volumeLabel">The volume label.</param>
        /// <returns></returns>
        public static ExFatPathFilesystem Format(Stream partitionStream, ExFatFormatOptions options, string volumeLabel = null)
        {
            var entryFilesystem = ExFatEntryFilesystem.Format(partitionStream, options, volumeLabel);
            return new ExFatPathFilesystem(entryFilesystem);
        }
    }
}
