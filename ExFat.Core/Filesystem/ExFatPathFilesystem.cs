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

    public class ExFatPathFilesystem : IDisposable
    {
        private readonly ExFatEntryFilesystem _entryFilesystem;
        private readonly Cache<string, ExFatFilesystemEntry> _entries;
        private const char Separator = '\\';

        public ExFatPathFilesystem(Stream partitionStream, ExFatFilesystemFlags flags = ExFatFilesystemFlags.Default)
            : this(new ExFatPartition(partitionStream), flags)
        { }

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

        public IEnumerable<string> EnumerateFiles(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Where(e => !e.IsDirectory).Select(e => GetPath(directoryPath, e));
        }

        public IEnumerable<string> EnumerateDirectories(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Where(e => e.IsDirectory).Select(e => GetPath(directoryPath, e));
        }

        public IEnumerable<string> EnumerateEntries(string directoryPath)
        {
            directoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(directoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Select(e => GetPath(directoryPath, e));
        }

        public DateTime GetCreationTime(string path) => GetSafeEntry(path).CreationTime;
        public DateTime GetCreationTimeUtc(string path) => GetSafeEntry(path).CreationTimeUtc;
        public DateTime GetLastWriteTime(string path) => GetSafeEntry(path).LastWriteTime;
        public DateTime GetLastWriteTimeUtc(string path) => GetSafeEntry(path).LastWriteTimeUtc;
        public DateTime GetLastAccessTime(string path) => GetSafeEntry(path).LastAccessTime;
        public DateTime GetLastAccessTimeUtc(string path) => GetSafeEntry(path).LastAccessTimeUtc;

        public void SetCreationTime(string path, DateTime creationTime) => UpdateSafeEntry(path, e => e.CreationDateTimeOffset = creationTime.ToLocalTime());
        public void SetCreationTimeUtc(string path, DateTime creationTime) => UpdateSafeEntry(path, e => e.CreationDateTimeOffset = creationTime.ToUniversalTime());
        public void SetLastWriteTime(string path, DateTime lastWriteTime) => UpdateSafeEntry(path, e => e.LastWriteDateTimeOffset = lastWriteTime.ToLocalTime());
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTime) => UpdateSafeEntry(path, e => e.LastWriteDateTimeOffset = lastWriteTime.ToUniversalTime());
        public void SetLastAccessTime(string path, DateTime lastAccessTime) => UpdateSafeEntry(path, e => e.LastAccessDateTimeOffset = lastAccessTime.ToLocalTime());
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

        public void CreateDirectory(string path)
        {
            CreateDirectoryEntry(path);
        }

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
    }
}
