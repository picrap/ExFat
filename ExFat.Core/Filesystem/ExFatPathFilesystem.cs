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
        private const char Separator = '\\';

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.Filesystem.ExFatPathFilesystem" /> class.
        /// </summary>
        /// <param name="partitionStream">The partition stream.</param>
        /// <param name="flags">The flags.</param>
        public ExFatPathFilesystem(Stream partitionStream, ExFatFilesystemFlags flags = ExFatFilesystemFlags.Default)
            : this(new ExFatPartition(partitionStream), flags)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExFat.Filesystem.ExFatPathFilesystem" /> class.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="flags">The flags.</param>
        public ExFatPathFilesystem(ExFatPartition partition, ExFatFilesystemFlags flags = ExFatFilesystemFlags.Default)
            : this(new ExFatEntryFilesystem(partition, flags))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatPathFilesystem"/> class.
        /// </summary>
        /// <param name="entryFilesystem">The entry filesystem.</param>
        public ExFatPathFilesystem(ExFatEntryFilesystem entryFilesystem)
        {
            _entryFilesystem = entryFilesystem;
            _entries = new Cache<string, ExFatFilesystemEntry>(Environment.ProcessorCount * (8 + 100));
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

            if (_entries.TryGetValue(path, out var entry))
                return entry;

            var pn = GetParentAndName(path);
            entry = GetEntry(pn.Item1, pn.Item2);
            _entries[path] = entry;
            return entry;
        }

        private static Tuple<string, string> GetParentAndName(string path)
        {
            var separatorIndex = path.LastIndexOf(Separator);
            if (separatorIndex < 0)
                return Tuple.Create("", path);
            return Tuple.Create(path.Substring(0, separatorIndex), path.Substring(separatorIndex + 1));
        }

        private static string CleanupPath(string path)
        {
            path = path.TrimEnd(Separator);
            return path;
        }

        private ExFatFilesystemEntry GetEntry(string parentPath, string childName)
        {
            var parentEntry = GetEntry(parentPath);
            if (parentEntry == null)
                return null;
            return _entryFilesystem.FindChild(parentEntry, childName);
        }

        private static string GetPath(string parentPath, ExFatFilesystemEntry entry)
        {
            return $"{parentPath}{Separator}{entry.MetaEntry.ExtensionsFileName}";
        }

        private ExFatFilesystemEntry GetSafeDirectory(string directoryPath)
        {
            var directory = GetEntry(directoryPath);
            if (directory == null)
                throw new DirectoryNotFoundException();
            if (!directory.IsDirectory)
                throw new IOException();
            return directory;
        }

        private ExFatFilesystemEntry GetSafeEntry(string path)
        {
            var entry = GetEntry(path);
            if (entry == null)
                throw new FileNotFoundException();
            return entry;
        }

        private void UpdateSafeEntry(string path, Action<ExFatFilesystemEntry> update)
        {
            var entry = GetSafeEntry(path);
            update(entry);
            _entryFilesystem.Update(entry);
        }

        /// <summary>
        /// Enumerates the files.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IEnumerable<string> EnumerateFiles(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Where(e => !e.IsDirectory).Select(e => GetPath(directoryPath, e));
        }

        /// <summary>
        /// Enumerates the directories.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IEnumerable<string> EnumerateDirectories(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Where(e => e.IsDirectory).Select(e => GetPath(directoryPath, e));
        }

        /// <summary>
        /// Enumerates the entries.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IEnumerable<string> EnumerateEntries(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Select(e => GetPath(directoryPath, e));
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

        private ExFatFilesystemEntry CreateDirectoryEntry(string path)
        {
            var existingDirectory = GetEntry(path);
            if (existingDirectory != null)
                return existingDirectory;
            var pn = GetParentAndName(path);
            var parentDirectory = CreateDirectoryEntry(pn.Item1);
            var directoryEntry = _entryFilesystem.CreateDirectory(parentDirectory, pn.Item2);
            _entries[path] = directoryEntry;
            return directoryEntry;
        }

        /// <summary>
        /// Creates a directory under the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CreateDirectory(string path)
        {
            CreateDirectoryEntry(path);
        }

        /// <summary>
        /// Deletes the specified entry at given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.IOException"></exception>
        public void Delete(string path)
        {
            path = CleanupPath(path);
            var entry = GetSafeEntry(path);
            if (entry.IsDirectory)
            {
                if (_entryFilesystem.EnumerateFileSystemEntries(entry).Any())
                    throw new IOException();
            }
            _entries[path] = null;
            _entryFilesystem.Delete(entry);
        }

        /// <summary>
        /// Deletes the tree at given path.
        /// If the entry is a file, simply deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void DeleteTree(string path)
        {
            path = CleanupPath(path);
            var entry = GetSafeEntry(path);
            if (entry.IsDirectory)
            {
                foreach (var childPath in EnumerateEntries(path))
                    DeleteTree(childPath);
            }
            Delete(path);
        }

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public ExFatEntryInformation GetInformation(string path)
        {
            var entry = GetEntry(path);
            if (entry == null)
                return null;
            return new ExFatEntryInformation(_entryFilesystem, entry);
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
            var pn = GetParentAndName(path);
            var parentEntry = GetEntry(pn.Item1);
            if (parentEntry == null)
                throw new DirectoryNotFoundException();
            var child = _entryFilesystem.FindChild(parentEntry, pn.Item2);
            // not existing?
            if (child == null)
            {
                if (mode == FileMode.Append || mode == FileMode.Open || mode == FileMode.Truncate)
                    throw new FileNotFoundException();
                return _entryFilesystem.CreateFile(parentEntry, pn.Item2);
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
    }
}
