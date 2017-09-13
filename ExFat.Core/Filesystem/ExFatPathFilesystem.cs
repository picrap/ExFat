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
        private class Path
        {
            private readonly string[] _allParts;

            public int Length { get; }

            public string Name => Length > 0 ? _allParts[Length - 1] : "";

            public Path(string[] parts, int count)
            {
                _allParts = parts;
                Length = count;
            }

            public Path GetParent()
            {
                if (Length == 0)
                    return null;
                return new Path(_allParts, Length - 1);
            }

            public string ToLiteral(char separator)
            {
                return string.Join(separator.ToString(), _allParts.Take(Length));
            }
        }

        private class Node
        {
            private readonly ExFatPathFilesystem _filesystem;
            private readonly IDictionary<string, Node> _children = new Dictionary<string, Node>();

            public Path Path { get; }

            public ExFatFilesystemEntry Entry { get; }

            public long Generation { get; private set; }

            public Node(Path path, ExFatFilesystemEntry entry, ExFatPathFilesystem filesystem)
            {
                _filesystem = filesystem;
                Generation = _filesystem.GetNextGeneration();
                Path = path;
                Entry = entry;
            }

            public Node GetChild(string name)
            {
                if (!_children.TryGetValue(name, out var node))
                    return null;
                if (_filesystem.HasExpired(node.Generation))
                {
                    _children.Remove(name);
                    return null;
                }
                node.Generation = _filesystem.GetNextGeneration();
                return node;
            }

            public Node NewChild(Path path, ExFatFilesystemEntry entry)
            {
                var child = new Node(path, entry, _filesystem);
                _children[path.Name] = child;
                return child;
            }

            public void RemoveChild(string name)
            {
                _children.Remove(name);
            }
        }

        private readonly ExFatEntryFilesystem _entryFilesystem;
        private readonly object _entriesLock = new object();
        private readonly char[] _separators;

        private readonly Node _rootNode;

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
            _separators = pathSeparators ?? DefaultSeparators;
            _rootNode = new Node(new Path(new[] { "" }, 0), _entryFilesystem.RootDirectory, this);
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _entryFilesystem.Dispose();
        }

        private long _generation;
        private readonly int _generationExpiry = Environment.ProcessorCount * (8 + 100);

        private long GetNextGeneration()
        {
            return ++_generation;
        }

        private bool HasExpired(long generation)
        {
            return _generation - generation > _generationExpiry;
        }

        private Node GetNode(Path path)
        {
            if (path == null)
                throw new ArgumentNullException();
            if (path.Length == 0)
                return _rootNode;

            lock (_entriesLock)
            {
                var parentNode = GetNode(path.GetParent());
                if (parentNode == null)
                    return null;

                var node = parentNode.GetChild(path.Name);
                if (node != null)
                    return node;

                return GetNode(parentNode, path);
            }
        }

        private Node GetNode(Node parentNode, Path path)
        {
            var filesystemEntry = _entryFilesystem.FindChild(parentNode.Entry, path.Name);
            return parentNode.NewChild(path, filesystemEntry);
        }

        private void ClearEntry(Path path)
        {
            var entry = GetNode(path.GetParent());
            if (entry != null)
                entry.RemoveChild(path.Name);
        }

        private Tuple<string, string> GetParentAndName(string cleanPath)
        {
            var separatorIndex = cleanPath.LastIndexOfAny(_separators);
            if (separatorIndex < 0)
                return Tuple.Create("", cleanPath);
            return Tuple.Create(cleanPath.Substring(0, separatorIndex), cleanPath.Substring(separatorIndex + 1));
        }

        private Path CleanupPath(string path)
        {
            var parts = path.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            return new Path(parts, parts.Length);
        }

        private string GetPath(string cleanParentPath, ExFatFilesystemEntry entry)
        {
            var fileName = entry.MetaEntry.ExtensionsFileName;
            return GetPath(cleanParentPath, fileName);
        }

        private string GetPath(string cleanParentPath, string fileName)
        {
            // on root entries, the direct name is returned
            if (cleanParentPath == "")
                return fileName;
            return $"{cleanParentPath}{_separators[0]}{fileName}";
        }

        private ExFatFilesystemEntry GetSafeDirectory(Path directoryPath)
        {
            var directory = GetNode(directoryPath);
            if (directory == null)
                throw new DirectoryNotFoundException();
            if (!directory.Entry.IsDirectory)
                throw new IOException();
            return directory.Entry;
        }

        private Node GetSafeNode(Path path)
        {
            var entry = GetNode(path);
            if (entry == null)
                throw new FileNotFoundException();
            return entry;
        }

        private Node GetSafeNode(string path) => GetSafeNode(CleanupPath(path));

        private void UpdateSafeEntry(Path path, Action<ExFatFilesystemEntry> update)
        {
            var entry = GetSafeNode(path);
            update(entry.Entry);
            _entryFilesystem.Update(entry.Entry);
        }

        private void UpdateSafeEntry(string path, Action<ExFatFilesystemEntry> update) => UpdateSafeEntry(CleanupPath(path), update);

        /// <summary>
        /// Enumerates the entries.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IEnumerable<ExFatEntryInformation> EnumerateEntries(string directoryPath)
        {
            var cleanDirectoryPath = CleanupPath(directoryPath);
            var directory = GetSafeDirectory(cleanDirectoryPath);
            return _entryFilesystem.EnumerateFileSystemEntries(directory).Select(e => new ExFatEntryInformation(_entryFilesystem, e, GetPath(cleanDirectoryPath.ToLiteral(_separators[0]), e)));
        }

        /// <summary>
        /// Gets the creation time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetCreationTime(string path) => GetSafeNode(path).Entry.CreationTime;
        /// <summary>
        /// Gets the creation time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetCreationTimeUtc(string path) => GetSafeNode(path).Entry.CreationTimeUtc;
        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastWriteTime(string path) => GetSafeNode(path).Entry.LastWriteTime;
        /// <summary>
        /// Gets the last write time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastWriteTimeUtc(string path) => GetSafeNode(path).Entry.LastWriteTimeUtc;
        /// <summary>
        /// Gets the last access time.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastAccessTime(string path) => GetSafeNode(path).Entry.LastAccessTime;
        /// <summary>
        /// Gets the last access time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetLastAccessTimeUtc(string path) => GetSafeNode(path).Entry.LastAccessTimeUtc;

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

        private Node CreateDirectoryEntry(Path path)
        {
            var existingDirectory = GetNode(path);
            if (existingDirectory != null)
                return existingDirectory;
            var parentDirectory = CreateDirectoryEntry(path.GetParent());
            var directoryEntry = _entryFilesystem.CreateDirectory(parentDirectory.Entry, path.Name);
            return parentDirectory.NewChild(path, directoryEntry);
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
            var node = GetSafeNode(path);
            if (node.Entry.IsDirectory)
            {
                if (_entryFilesystem.EnumerateFileSystemEntries(node.Entry).Any())
                    throw new IOException();
            }
            _entryFilesystem.Delete(node.Entry);
        }

        /// <summary>
        /// Deletes the tree at given path.
        /// If the entry is a file, simply deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void DeleteTree(string path)
        {
            var cleanPath = CleanupPath(path);
            var entry = GetSafeNode(cleanPath);
            if (entry.Entry.IsDirectory)
            {
                foreach (var childPath in EnumerateEntries(cleanPath.ToLiteral(_separators[0])))
                    DeleteTree(childPath.Path);
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
            var cleanPath = CleanupPath(path);
            var entry = GetNode(cleanPath);
            if (entry == null)
                return null;
            return new ExFatEntryInformation(_entryFilesystem, entry.Entry, cleanPath.ToLiteral(_separators[0]));
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
            var parentEntry = GetNode(cleanPath.GetParent());
            if (parentEntry == null)
                throw new DirectoryNotFoundException();
            var child = _entryFilesystem.FindChild(parentEntry.Entry, cleanPath.Name);
            // not existing?
            if (child == null)
            {
                if (mode == FileMode.Append || mode == FileMode.Open || mode == FileMode.Truncate)
                    throw new FileNotFoundException();
                return _entryFilesystem.CreateFile(parentEntry.Entry, cleanPath.Name);
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
            var cleanTargetDirectory = targetDirectory == null ? cleanSourcePath.GetParent() : CleanupPath(targetDirectory);

            var sourceEntry = GetNode(cleanSourcePath);
            if (sourceEntry == null)
                throw new FileNotFoundException();
            var targetDirectoryEntry = GetNode(cleanTargetDirectory);
            if (targetDirectoryEntry == null)
                throw new FileNotFoundException();

            var cleanTargetPath = cleanTargetDirectory;
            cleanTargetPath = GetPath(cleanTargetPath, targetName ?? cleanSourcePath.Name);
            _entryFilesystem.Move(sourceEntry, targetDirectoryEntry, targetName);
            Unregister(cleanTargetPath);
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
