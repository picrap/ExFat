// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Buffers;
    using IO;
    using Partition;
    using Partition.Entries;

    public class ExFatFilesystem : IDisposable
    {
        private readonly ExFatFilesystemFlags _flags;
        private readonly ExFatPartition _partition;
        private readonly object _lock = new object();

        private ExFatFilesystemEntry _rootDirectory;

        /// <summary>
        /// Gets the root directory.
        /// </summary>
        /// <value>
        /// The root directory.
        /// </value>
        public ExFatFilesystemEntry RootDirectory
        {
            get
            {
                if (_rootDirectory == null)
                    _rootDirectory = CreateRootDirectory();
                return _rootDirectory;
            }
        }

        public ExFatFilesystem(Stream partitionStream, ExFatFilesystemFlags flags = ExFatFilesystemFlags.Default)
        {
            _flags = flags;
            _partition = new ExFatPartition(partitionStream);
        }

        public void Dispose()
        {
            _partition.Dispose();
        }

        private ExFatFilesystemEntry CreateRootDirectory()
        {
            return new ExFatFilesystemEntry(dataDescriptorOverride: _partition.RootDirectoryDataDescriptor, attributesOverride: ExFatFileAttributes.Directory);
        }

        public IEnumerable<ExFatFilesystemEntry> EnumerateFileSystemEntries(ExFatFilesystemEntry directoryEntry)
        {
            if (!directoryEntry.IsDirectory)
                throw new InvalidOperationException();

            using (var directory = OpenDirectory(directoryEntry))
            {
                foreach (var metaEntry in directory.GetMetaEntries())
                {
                    // keep only file entries
                    if (metaEntry.Primary is FileExFatDirectoryEntry)
                        yield return new ExFatFilesystemEntry(metaEntry);
                }
            }
        }

        private ExFatDirectory OpenDirectory(ExFatFilesystemEntry entry)
        {
            return new ExFatDirectory(OpenData(entry, FileAccess.ReadWrite), true);
        }

        public ExFatFilesystemEntry FindChild(ExFatFilesystemEntry directoryEntry, string name)
        {
            if (!directoryEntry.IsDirectory)
                throw new InvalidOperationException();

            // namehash is fun, but what efficiency do we gain?
            using (var directory = OpenDirectory(directoryEntry))
                return FindChild(directory, name);
        }

        private ExFatFilesystemEntry FindChild(ExFatDirectory directory, string name)
        {
            var nameHash = _partition.ComputeNameHash(name);
            foreach (var metaEntry in directory.GetMetaEntries())
            {
                var streamExtension = metaEntry.SecondaryStreamExtension;
                // keep only file entries
                if (streamExtension != null && streamExtension.NameHash.Value == nameHash && metaEntry.ExtensionsFileName == name)
                    return new ExFatFilesystemEntry(metaEntry);
            }
            return null;
        }

        /// <summary>
        /// Opens the specified entry.
        /// </summary>
        /// <param name="fileEntry">The entry.</param>
        /// <param name="access">The access.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Stream OpenFile(ExFatFilesystemEntry fileEntry, FileAccess access)
        {
            if (fileEntry.IsDirectory)
                throw new InvalidOperationException();

            return OpenData(fileEntry, access);
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="parentDirectory">The parent directory.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public Stream CreateFile(ExFatFilesystemEntry parentDirectory, string fileName)
        {
            if (!parentDirectory.IsDirectory)
                throw new InvalidOperationException();

            using (var directory = OpenDirectory(parentDirectory))
            {
                var existingFile = FindChild(directory, fileName);
                if (existingFile != null)
                {
                    var stream = OpenData(existingFile, FileAccess.ReadWrite);
                    stream.SetLength(0);
                    return stream;
                }

                var fileEntry = CreateEntry(fileName, FileAttributes.Archive);
                directory.AddEntry(fileEntry.MetaEntry);
                return OpenData(fileEntry, FileAccess.ReadWrite);
            }
        }

        private PartitionStream OpenData(ExFatFilesystemEntry fileEntry, FileAccess access)
        {
            return _partition.OpenDataStream(fileEntry.DataDescriptor, access, d => OnDisposed(fileEntry, access, d));
        }

        private void OnDisposed(ExFatFilesystemEntry entry, FileAccess descriptor, DataDescriptor dataDescriptor)
        {
            if (entry?.MetaEntry == null)
                return;

            DateTimeOffset? now = null;
            var file = (FileExFatDirectoryEntry)entry.MetaEntry.Primary;

            // if file was open for reading and the flag is set, the entry is updated
            if (descriptor.HasAny(FileAccess.Read) && _flags.HasAny(ExFatFilesystemFlags.UpdateLastAccessTime))
            {
                now = DateTimeOffset.Now;
                file.LastAccessDateTimeOffset.Value = now.Value;
            }

            // when it was open for writing, its characteristics may have changed, so we update them
            if (descriptor.HasAny(FileAccess.Write))
            {
                now = now ?? DateTimeOffset.Now;
                file.FileAttributes.Value |= ExFatFileAttributes.Archive;
                file.LastWriteDateTimeOffset.Value = now.Value;
                var stream = entry.MetaEntry.SecondaryStreamExtension;
                if (dataDescriptor.Contiguous)
                    stream.GeneralSecondaryFlags.Value |= ExFatGeneralSecondaryFlags.NoFatChain;
                else
                    stream.GeneralSecondaryFlags.Value &= ~ExFatGeneralSecondaryFlags.NoFatChain;
                stream.FirstCluster.Value = (UInt32)dataDescriptor.FirstCluster.Value;
                stream.ValidDataLength.Value = dataDescriptor.Length.Value;
                stream.DataLength.Value = dataDescriptor.Length.Value;
            }

            // now has value only if it was used before, so we spare a flag :)
            if (now.HasValue)
            {
                _partition.UpdateEntry(entry.MetaEntry);
            }
        }

        /// <summary>
        /// Creates a <see cref="ExFatFilesystemEntry"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        private ExFatFilesystemEntry CreateEntry(string name, FileAttributes attributes)
        {
            var now = DateTimeOffset.Now;
            var entries = new List<ExFatDirectoryEntry>
            {
                new FileExFatDirectoryEntry(new Buffers.Buffer(new byte[32]))
                {
                    EntryType = { Value = ExFatDirectoryEntryType.File},
                    FileAttributes = {Value = (ExFatFileAttributes) attributes},
                    CreationDateTimeOffset = {Value = now},
                    LastWriteDateTimeOffset = {Value = now},
                    LastAccessDateTimeOffset = {Value = now},
                },
                new StreamExtensionExFatDirectoryEntry(new Buffers.Buffer(new byte[32]))
                {
                    EntryType = { Value = ExFatDirectoryEntryType.Stream},
                    GeneralSecondaryFlags = {Value = ExFatGeneralSecondaryFlags.ClusterAllocationPossible},
                    NameLength = {Value = (byte) name.Length},
                    NameHash = {Value = _partition.ComputeNameHash(name)},
                }
            };
            for (int nameIndex = 0; nameIndex < name.Length; nameIndex += 15)
            {
                var namePart = name.Substring(nameIndex, Math.Min(15, name.Length - nameIndex));
                entries.Add(new FileNameExtensionExFatDirectoryEntry(new Buffers.Buffer(new byte[32]))
                {
                    EntryType = { Value = ExFatDirectoryEntryType.FileName },
                    FileName = { Value = namePart }
                });
            }
            var metaEntry = new ExFatMetaDirectoryEntry(entries);
            var entry = new ExFatFilesystemEntry(metaEntry);
            return entry;
        }

        /// <summary>
        /// Creates a directory or returns the existing.
        /// </summary>
        /// <param name="parentDirectoryEntry">The parent directory.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException"></exception>
        public ExFatFilesystemEntry CreateDirectory(ExFatFilesystemEntry parentDirectoryEntry, string directoryName)
        {
            if (!parentDirectoryEntry.IsDirectory)
                throw new InvalidOperationException();

            lock (_lock)
            {
                using (var parentDirectory = OpenDirectory(parentDirectoryEntry))
                {
                    var existingEntry = FindChild(parentDirectory, directoryName);
                    if (existingEntry != null)
                    {
                        if (!existingEntry.IsDirectory)
                            throw new IOException();
                        return existingEntry;
                    }

                    var directoryEntry = CreateEntry(directoryName, FileAttributes.Directory);
                    parentDirectory.AddEntry(directoryEntry.MetaEntry);
                    using (var directoryStream = OpenData(directoryEntry, FileAccess.ReadWrite))
                    {
                        var emptyEntry = new byte[32];
                        directoryStream.Write(emptyEntry, 0, emptyEntry.Length);
                    }
                    return directoryEntry;
                }
            }
        }

        public void Delete(ExFatFilesystemEntry entry)
        {
            _partition.Deallocate(entry.DataDescriptor);
            foreach (var e in entry.MetaEntry.Entries)
                e.EntryType.Value &= ~ExFatDirectoryEntryType.InUse;
            _partition.UpdateEntry(entry.MetaEntry);
        }
    }
}
