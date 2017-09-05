// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IO;
    using Partition;
    using Partition.Entries;

    public class ExFatFilesystem : IDisposable
    {
        private readonly ExFatFilesystemFlags _flags;
        private readonly ExFatPartition _partition;

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
            return new ExFatDirectory(_partition.OpenDataStream(entry.DataDescriptor, FileAccess.Read), true);
        }

        public ExFatFilesystemEntry FindChild(ExFatFilesystemEntry directoryEntry, string name)
        {
            if (!directoryEntry.IsDirectory)
                throw new InvalidOperationException();

            // namehash is fun, but what efficiency do we gain?
            var nameHash = _partition.ComputeNameHash(name);
            using (var directory = OpenDirectory(directoryEntry))
            {
                foreach (var metaEntry in directory.GetMetaEntries())
                {
                    var streamExtension = metaEntry.SecondaryStreamExtension;
                    // keep only file entries
                    if (streamExtension != null && streamExtension.NameHash.Value == nameHash && metaEntry.ExtensionsFileName == name)
                        return new ExFatFilesystemEntry(metaEntry);
                }
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
        public Stream Open(ExFatFilesystemEntry fileEntry, FileAccess access)
        {
            if (!fileEntry.IsDirectory)
                throw new InvalidOperationException();

            return _partition.OpenDataStream(fileEntry.DataDescriptor, access, d => OnDisposed(fileEntry, access, d));
        }

        private void OnDisposed(ExFatFilesystemEntry entry, FileAccess descriptor, DataDescriptor dataDescriptor)
        {
            DateTime? now = null;
            var file = (FileExFatDirectoryEntry)entry.Entry.Primary;

            // if file was open for reading and the flag is set, the entry is updated
            if (descriptor.HasAny(FileAccess.Read) && _flags.HasAny(ExFatFilesystemFlags.UpdateLastAccessTime))
            {
                now = DateTime.Now;
                file.LastAccessDateTime.Value = now.Value;
            }

            // when it was open for writing, its characteristics may have changed, so we update them
            if (descriptor.HasAny(FileAccess.Write))
            {
                now = now ?? DateTime.Now;
                file.LastWriteTime.Value = now.Value;
                var stream = entry.Entry.SecondaryStreamExtension;
                if (dataDescriptor.Contiguous)
                    stream.GeneralSecondaryFlags.Value |= ExFatGeneralSecondaryFlags.NoFatChain;
                else
                    stream.GeneralSecondaryFlags.Value &= ~ExFatGeneralSecondaryFlags.NoFatChain;
                stream.ValidDataLength.Value = dataDescriptor.Length.Value;
                stream.DataLength.Value = dataDescriptor.Length.Value;
            }

            // now has value only if it was used before, so we spare a flag :)
            if (now.HasValue)
            {
                _partition.UpdateEntry(entry.Entry);
            }
        }
    }
}
