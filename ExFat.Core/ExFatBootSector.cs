namespace ExFat.Core
{
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Buffer;

    public class ExFatBootSector
    {
        public static readonly byte[] DefaultMarker = new byte[] { 0xEB, 0x76, 0x90 };
        public const string ExFatFileSystemName = "EXFAT   ";

        private byte[] _bytes;

        public BufferBytes Marker { get; }
        public BufferByteString FileSystemName { get; }
        public BufferUInt64 VolumeLength { get; }
        public BufferUInt32 FatOffset { get; }
        public BufferUInt32 FatLength { get; }

        /// <summary>
        /// Returns true if this boot sector is valid (better check this after reading it).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => Marker.SequenceEqual(DefaultMarker) && FileSystemName.Value == ExFatFileSystemName;

        public ExFatBootSector()
        {
            _bytes = new byte[512];
            Marker = new BufferBytes(_bytes, 0, 3);
            FileSystemName = new BufferByteString(_bytes, 3, 8);
            VolumeLength = new BufferUInt64(_bytes, 72);
            FatOffset = new BufferUInt32(_bytes, 80);
            FatLength = new BufferUInt32(_bytes, 84);
        }

        public void Read(Stream stream)
        {
            stream.Read(_bytes, 0, _bytes.Length);
        }
    }
}
