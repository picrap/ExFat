// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using IO;
    using Partition.Entries;

    [DebuggerDisplay("{" + nameof(DebugLiteral) + "}")]
    public class ExFatFilesystemEntry
    {
        private readonly DataDescriptor _dataDescriptorOverride;
        private FileExFatDirectoryEntry FileEntry => Entry.Primary as FileExFatDirectoryEntry;
        private readonly ExFatFileAttributes? _attributesOverride;
        private string DebugLiteral => Name + (IsDirectory ? "/" : "");

        internal ExFatMetaDirectoryEntry Entry { get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public FileAttributes Attributes
        {
            get
            {
                if (_attributesOverride.HasValue)
                    return (FileAttributes)_attributesOverride.Value;
                // this is always the case
                if (FileEntry != null)
                    return (FileAttributes)FileEntry.FileAttributes.Value;
                return 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is directory.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory => Attributes.HasAny(FileAttributes.Directory);

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => Entry.ExtensionsFileName;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => (long)Entry.SecondaryStreamExtension.DataLength.Value;

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        public DateTime CreationDate => FileEntry.CreationTime.Value;

        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public DateTime LastWriteTime => FileEntry.LastWriteTime.Value;

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        /// <value>
        /// The last access time.
        /// </value>
        public DateTime LastAccessTime => FileEntry.LastAccessTime.Value;

        /// <summary>
        /// Gets the data descriptor.
        /// </summary>
        /// <value>
        /// The data descriptor.
        /// </value>
        public DataDescriptor DataDescriptor => _dataDescriptorOverride ?? Entry.DataDescriptor;

        internal ExFatFilesystemEntry(ExFatMetaDirectoryEntry entry = null, DataDescriptor dataDescriptorOverride = null, ExFatFileAttributes? attributesOverride = null)
        {
            _dataDescriptorOverride = dataDescriptorOverride;
            Entry = entry;
            _attributesOverride = attributesOverride;
        }
    }
}
