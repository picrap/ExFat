namespace ExFat.Core.Entries
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Groups entries by primary entry
    /// </summary>
    public class ExFatMetaDirectoryEntry
    {
        public List<ExFatDirectoryEntry> Entries { get; } = new List<ExFatDirectoryEntry>();

        /// <summary>
        /// Gets the primary <see cref="ExFatDirectoryEntry"/> directory entry.
        /// </summary>
        /// <value>
        /// The primary.
        /// </value>
        public ExFatDirectoryEntry Primary => Entries.FirstOrDefault();
        /// <summary>
        /// Gets the secondary <see cref="ExFatDirectoryEntry"/> entries, if any.
        /// </summary>
        /// <value>
        /// The secondaries.
        /// </value>
        public IEnumerable<ExFatDirectoryEntry> Secondaries => Entries.Skip(1);

        /// <summary>
        /// Gets the <see cref="StreamExtensionExFatDirectoryEntry"/>, if any.
        /// </summary>
        /// <value>
        /// The secondary stream extension.
        /// </value>
        public StreamExtensionExFatDirectoryEntry SecondaryStreamExtension => Secondaries.OfType<StreamExtensionExFatDirectoryEntry>().FirstOrDefault();
        /// <summary>
        /// Gets the secondary <see cref="FileNameExtensionExFatDirectoryEntry"/>, if any.
        /// </summary>
        /// <value>
        /// The secondary file name extensions.
        /// </value>
        public IEnumerable<FileNameExtensionExFatDirectoryEntry> SecondaryFileNameExtensions => Secondaries.OfType<FileNameExtensionExFatDirectoryEntry>();

        /// <summary>
        /// Gets the extended file name, based on <see cref="SecondaryFileNameExtensions"/>.
        /// </summary>
        /// <value>
        /// The name of the extensions file.
        /// </value>
        public string ExtensionsFileName => string.Join("", SecondaryFileNameExtensions.Select(s => s.FileName.Value));

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatMetaDirectoryEntry"/> class.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public ExFatMetaDirectoryEntry(IEnumerable<ExFatDirectoryEntry> entries)
        {
            Entries.AddRange(entries);
        }
    }
}