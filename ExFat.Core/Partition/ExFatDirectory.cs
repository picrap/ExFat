// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Entries;
    using IO;
    using Buffer = Buffers.Buffer;

    public class ExFatDirectory : IDisposable
    {
        private readonly PartitionStream _directoryStream;
        private readonly bool _ownsStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatDirectory"/> class.
        /// </summary>
        /// <param name="directoryStream">The directory stream.</param>
        /// <param name="ownsStream">if set to <c>true</c> [owns stream].</param>
        public ExFatDirectory(PartitionStream directoryStream, bool ownsStream)
        {
            _directoryStream = directoryStream;
            _ownsStream = ownsStream;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_ownsStream)
                _directoryStream.Dispose();
        }

        /// <summary>
        /// Gets the entries, totally raw (includes the deleted entries).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExFatDirectoryEntry> GetEntries()
        {
            lock (_directoryStream)
            {
                if (_directoryStream.CanSeek)
                    _directoryStream.Seek(0, SeekOrigin.Begin);
                for (var offset = 0L; ; offset += 32)
                {
                    var entryBytes = new byte[32];
                    // cluster offset before reading data, since it's the start
                    var clusterPosition = _directoryStream.ClusterPosition;
                    if (_directoryStream.Read(entryBytes, 0, entryBytes.Length) != 32)
                        break;
                    var directoryEntry = ExFatDirectoryEntry.Create(new Buffer(entryBytes), offset, clusterPosition);
                    if (directoryEntry != null)
                        yield return directoryEntry;
                }
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

        /// <summary>
        /// Finds the available slot.
        /// </summary>
        /// <param name="entriesCount">The entries count.</param>
        /// <returns></returns>
        private long FindAvailableSlot(int entriesCount)
        {
            lock (_directoryStream)
            {
                if (!_directoryStream.CanSeek)
                    throw new InvalidOperationException();

                long availableSlot = -1;
                int availableCount = 0;
                _directoryStream.Seek(0, SeekOrigin.Begin);
                for (var offset = 0L; ; offset += 32)
                {
                    var typeByte = _directoryStream.ReadByte();
                    // when we reach the end, we can append from here
                    if (typeByte == -1)
                    {
                        if (availableSlot == -1)
                            availableSlot = offset;
                        return availableSlot;
                    }

                    var type = (ExFatDirectoryEntryType)typeByte;
                    if (type.HasAny(ExFatDirectoryEntryType.InUse))
                    {
                        availableSlot = -1;
                    }
                    else
                    {
                        if (availableSlot == -1)
                        {
                            availableSlot = offset;
                            availableCount = 0;
                        }
                        if (++availableCount == entriesCount)
                            return availableSlot;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the entry to directory.
        /// </summary>
        /// <param name="metaEntry">The meta entry.</param>
        public void AddEntry(ExFatMetaDirectoryEntry metaEntry)
        {
            lock (_directoryStream)
            {
                var availableSlot = FindAvailableSlot(metaEntry.Entries.Count);
                _directoryStream.Seek(availableSlot, SeekOrigin.Begin);
                foreach (var entry in metaEntry.Entries)
                    entry.EntryType.Value |= ExFatDirectoryEntryType.InUse;
                metaEntry.Write(_directoryStream);
            }
        }

        /// <summary>
        /// Deletes the entry.
        /// </summary>
        /// <param name="metaEntry">The meta entry.</param>
        public void DeleteEntry(ExFatMetaDirectoryEntry metaEntry)
        {
            lock (_directoryStream)
            {
                _directoryStream.Seek(metaEntry.Primary.Position, SeekOrigin.Begin);
                foreach (var entry in metaEntry.Entries)
                    entry.EntryType.Value &= ~ExFatDirectoryEntryType.InUse;
                metaEntry.Write(_directoryStream);
            }
        }
    }
}
