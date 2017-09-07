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

            var separatorIndex = path.LastIndexOf(Separator);
            if (separatorIndex < 0)
                entry = GetEntry("", path);
            else
                entry = GetEntry(path.Substring(0, separatorIndex), path.Substring(separatorIndex + 1));
            _entries[path] = entry;
            return entry;
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
    }
}
