namespace ExFat.Core
{
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The ExFAT filesystem.
    /// The class is a quite low-level manipulator
    /// TODO: come up with a better name :)
    /// </summary>
    public class ExFatFS
    {
        private readonly Stream _partitionStream;

        public ExFatFS(Stream partitionStream)
        {
            _partitionStream = partitionStream;
        }

        public ExFatBootSector ReadBootSector(Stream partitionStream)
        {
            partitionStream.Seek(0, SeekOrigin.Begin);
            var bootSector = new ExFatBootSector();
            bootSector.Read(partitionStream);
            return bootSector;
        }
    }
}