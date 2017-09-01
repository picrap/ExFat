namespace ExFat.Core
{
    using System.IO;
    using Buffers;
    using Entries;

    public class ExFatDirectory
    {
        public ExFatDirectory(Stream directoryStream)
        {
            for (; ; )
            {
                var entryBytes = new byte[32];
                if (directoryStream.Read(entryBytes, 0, entryBytes.Length) != 32)
                    break;
                var directoryEntry = ExFatDirectoryEntry.Create(new Buffer(entryBytes));
                if (directoryEntry == null)
                    break;
            }
        }
    }
}
