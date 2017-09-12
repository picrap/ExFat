// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Information about file system entry.
    /// High-level, related to <see cref="ExFatPathFilesystem"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(Path) + "}")]
    public class ExFatEntryInformation
    {
        private readonly ExFatEntryFilesystem _entryFilesystem;
        private readonly ExFatFilesystemEntry _entry;

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public FileAttributes Attributes
        {
            get { return _entry.Attributes; }
            set { _entry.Attributes = value; Update(); }
        }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTime CreationTime
        {
            get { return _entry.CreationDateTimeOffset.LocalDateTime; }
            set { _entry.CreationDateTimeOffset = value.ToLocalTime(); Update(); }
        }

        /// <summary>
        /// Gets or sets the creation time, UTC.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTime CreationTimeUtc
        {
            get { return _entry.CreationDateTimeOffset.UtcDateTime; }
            set { _entry.CreationDateTimeOffset = value.ToUniversalTime(); Update(); }
        }

        /// <summary>
        /// Gets or sets the last write time.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public DateTime LastWriteTime
        {
            get { return _entry.LastWriteDateTimeOffset.LocalDateTime; }
            set { _entry.LastWriteDateTimeOffset = value.ToLocalTime(); Update(); }
        }

        /// <summary>
        /// Gets or sets the last write time, UTC.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public DateTime LastWriteTimeUtc
        {
            get { return _entry.LastWriteDateTimeOffset.UtcDateTime; }
            set { _entry.LastWriteDateTimeOffset = value.ToUniversalTime(); Update(); }
        }

        /// <summary>
        /// Gets or sets the last write time.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public DateTime LastAccessTime
        {
            get { return _entry.LastAccessDateTimeOffset.LocalDateTime; }
            set { _entry.LastAccessDateTimeOffset = value.ToLocalTime(); Update(); }
        }

        /// <summary>
        /// Gets or sets the last write time, UTC.
        /// </summary>
        /// <value>
        /// The last write time.
        /// </value>
        public DateTime LastAccessTimeUtc
        {
            get { return _entry.LastAccessDateTimeOffset.UtcDateTime; }
            set { _entry.LastAccessDateTimeOffset = value.ToUniversalTime(); Update(); }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => _entry.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatEntryInformation"/> class.
        /// </summary>
        /// <param name="entryFilesystem">The entry filesystem.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="cleanPath">The path.</param>
        internal ExFatEntryInformation(ExFatEntryFilesystem entryFilesystem, ExFatFilesystemEntry entry, string cleanPath)
        {
            Path = cleanPath;
            _entryFilesystem = entryFilesystem;
            _entry = entry;
        }

        private void Update()
        {
            _entryFilesystem.Update(_entry);
        }
    }
}