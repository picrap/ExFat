namespace ExFat.Core
{
    using System.Collections.Generic;
    using System.IO;
    using Buffers;
    using Entries;

    public class ExFatDirectory
    {
        private readonly Stream _directoryStream;

        public ExFatDirectory(Stream directoryStream)
        {
            _directoryStream = directoryStream;
        }

        /// <summary>
        /// Gets the entries, totally raw (includes the deleted entries).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExFatDirectoryEntry> GetEntries()
        {
            if (_directoryStream.CanSeek)
                _directoryStream.Seek(0, SeekOrigin.Begin);
            for (; ; )
            {
                var entryBytes = new byte[32];
                if (_directoryStream.Read(entryBytes, 0, entryBytes.Length) != 32)
                    break;
                var directoryEntry = ExFatDirectoryEntry.Create(new Buffer(entryBytes));
                if (directoryEntry == null)
                    break;
                yield return directoryEntry;
            }
        }

        /// <summary>
        /// Gets the entries grouped: one primary followed by its secondaries.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExFatMetaDirectoryEntry> GetMetaEntries()
        {
            var entriesStack = new List<ExFatDirectoryEntry>();
            foreach (var directoryEntry in GetEntries())
            {
                if (!directoryEntry.InUse)
                    continue;

                if (directoryEntry.IsSecondary)
                    entriesStack.Add(directoryEntry);
                else
                {
                    if (entriesStack.Count > 0)
                        yield return new ExFatMetaDirectoryEntry(entriesStack);
                    entriesStack.Clear();
                    entriesStack.Add(directoryEntry);
                }
            }
            if (entriesStack.Count > 0)
                yield return new ExFatMetaDirectoryEntry(entriesStack);
        }
    }
}
