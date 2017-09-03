namespace ExFat.Core
{
    using System.IO;
    using System.Linq;
    using Buffers;

    public class ExFatBootSector
    {
        public static readonly byte[] DefaultMarker = new byte[] { 0xEB, 0x76, 0x90 };
        public const string ExFatFileSystemName = "EXFAT   ";

        private readonly byte[] _bytes;

        public BufferBytes Marker { get; }
        public BufferByteString FileSystemName { get; }

        /// <summary>
        /// Total number of Sectors.
        /// </summary>
        /// <value>
        /// The length of the volume.
        /// </value>
        public BufferUInt64 VolumeLength { get; }
        /// <summary>
        /// Sector address of 1st FAT.
        /// </summary>
        /// <value>
        /// The fat offset.
        /// </value>
        public BufferUInt32 FatOffset { get; }
        /// <summary>
        /// Size of FAT in sectors.
        /// </summary>
        /// <value>
        /// The length of the fat.
        /// </value>
        public BufferUInt32 FatLength { get; }
        /// <summary>
        /// Starting sector of cluster heap
        /// </summary>
        /// <value>
        /// The cluster offset.
        /// </value>
        public BufferUInt32 ClusterOffset { get; }
        /// <summary>
        /// Number of clusters.
        /// </summary>
        /// <value>
        /// The cluster count.
        /// </value>
        public BufferUInt32 ClusterCount { get; }
        /// <summary>
        /// First cluster of root directory.
        /// </summary>
        /// <value>
        /// The root directory.
        /// </value>
        public BufferUInt32 RootDirectory { get; }
        /// <summary>
        /// Gets the volume flags.
        /// Bit 0 – Active FAT
        ///         0 – 1st , 1 – 2nd
        /// Bit 1 – Volume Dirty
        ///         0 – Clean, 1- dirty
        /// Bits 2 & 3 – Media failure
        ///         0 – No failures, 1 – failures reported
        /// </summary>
        /// <value>
        /// The volume flags.
        /// </value>
        public BufferUInt16 VolumeFlags { get; }
        /// <summary>
        /// This is power of 2; Minimal value is 9; 2^9=512 bytes
        /// Bytes and maximum 2^12=4096 Bytes
        /// </summary>
        /// <value>
        /// The bytes per sector.
        /// </value>
        public BufferUInt8 BytesPerSector { get; }
        /// <summary>
        /// This is power of 2; Minimal value is 1; 2^0=1
        /// sector (512 Bytes) and maximum 32 MB cluster
        /// size in bytes
        /// </summary>
        /// <value>
        /// The sectors per cluster.
        /// </value>
        public BufferUInt8 SectorsPerCluster { get; }
        /// <summary>
        /// Either 1 or 2; if TexFAT is supported then it will be 2
        /// </summary>
        /// <value>
        /// The number of fats.
        /// </value>
        public BufferUInt8 NumberOfFats { get; }

        /// <summary>
        /// Returns true if this boot sector is valid (better check this after reading it).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => FileSystemName.Value == ExFatFileSystemName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatBootSector"/> class.
        /// </summary>
        public ExFatBootSector()
        {
            _bytes = new byte[512];
            var buffer = new Buffers.Buffer(_bytes);
            Marker = new BufferBytes(buffer, 0, 3);
            FileSystemName = new BufferByteString(buffer, 3, 8);
            VolumeLength = new BufferUInt64(buffer, 72);
            FatOffset = new BufferUInt32(buffer, 80);
            FatLength = new BufferUInt32(buffer, 84);
            ClusterOffset = new BufferUInt32(buffer, 88);
            ClusterCount = new BufferUInt32(buffer, 92);
            RootDirectory = new BufferUInt32(buffer, 96);
            VolumeFlags = new BufferUInt16(buffer, 106);
            BytesPerSector = new BufferUInt8(buffer, 108);
            SectorsPerCluster = new BufferUInt8(buffer, 109);
            NumberOfFats = new BufferUInt8(buffer, 110);
        }

        public void Read(Stream stream)
        {
            stream.Read(_bytes, 0, _bytes.Length);
        }
    }
}
