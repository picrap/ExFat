// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.IO;
    using Buffers;
    using Buffer = Buffers.Buffer;

    /// <summary>
    /// exFAT boot sector
    /// </summary>
    public class ExFatBootSector
    {
        /// <summary>
        /// The default JMP boot
        /// </summary>
        public static readonly byte[] DefaultJmpBoot = new byte[] {0xEB, 0x76, 0x90};
        /// <summary>
        /// The OEM name
        /// </summary>
        public const string ExFatOemName = "EXFAT   ";

        private readonly byte[] _bytes;

        /// <summary>
        /// Gets or sets the JMP boot.
        /// </summary>
        /// <value>
        /// The JMP boot.
        /// </value>
        public BufferBytes JmpBoot { get; }

        /// <summary>
        /// Gets or sets the OEM name.
        /// </summary>
        /// <value>
        /// The name of the oem.
        /// </value>
        public IValueProvider<string> OemName { get; }

        /// <summary>
        /// Total number of Sectors.
        /// </summary>
        /// <value>
        /// The length of the volume.
        /// </value>
        public IValueProvider<UInt64> VolumeLengthSectors { get; }

        /// <summary>
        /// Sector address of 1st FAT.
        /// </summary>
        /// <value>
        /// The fat offset.
        /// </value>
        public IValueProvider<UInt32> FatOffsetSector { get; }

        /// <summary>
        /// Size of FAT in sectors.
        /// </summary>
        /// <value>
        /// The length of the fat.
        /// </value>
        public IValueProvider<UInt32> FatLengthSectors { get; }

        /// <summary>
        /// Starting sector of cluster heap
        /// </summary>
        /// <value>
        /// The cluster offset.
        /// </value>
        public IValueProvider<UInt32> ClusterOffsetSector { get; }

        /// <summary>
        /// Number of clusters.
        /// </summary>
        /// <value>
        /// The cluster count.
        /// </value>
        public IValueProvider<UInt32> ClusterCount { get; }

        /// <summary>
        /// First cluster of root directory.
        /// </summary>
        /// <value>
        /// The root directory.
        /// </value>
        public IValueProvider<UInt32> RootDirectoryCluster { get; }

        /// <summary>
        /// Gets the volume flags.
        /// Bit 0 – Active FAT
        ///         0 – 1st , 1 – 2nd
        /// Bit 1 – Volume Dirty
        ///         0 – Clean, 1- dirty
        /// Bits 2 + 3 – Media failure
        ///         0 – No failures, 1 – failures reported
        /// </summary>
        /// <value>
        /// The volume flags.
        /// </value>
        public IValueProvider<UInt16> VolumeFlags { get; }

        /// <summary>
        /// This is power of 2; Minimal value is 9; 2^9=512 bytes
        /// Bytes and maximum 2^12=4096 Bytes
        /// </summary>
        /// <value>
        /// The bytes per sector.
        /// </value>
        public IValueProvider<UInt32> BytesPerSector { get; }

        /// <summary>
        /// This is power of 2; Minimal value is 1; 2^0=1
        /// sector (512 Bytes) and maximum 32 MB cluster
        /// size in bytes
        /// </summary>
        /// <value>
        /// The sectors per cluster.
        /// </value>
        public IValueProvider<UInt32> SectorsPerCluster { get; }

        /// <summary>
        /// Either 1 or 2; if TexFAT is supported then it will be 2
        /// </summary>
        /// <value>
        /// The number of fats.
        /// </value>
        public IValueProvider<byte> NumberOfFats { get; }

        /// <summary>
        /// Returns true if this boot sector is valid (better check this after reading it).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => OemName.Value == ExFatOemName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExFatBootSector"/> class.
        /// </summary>
        public ExFatBootSector()
        {
            _bytes = new byte[512];
            var buffer = new Buffer(_bytes);
            JmpBoot = new BufferBytes(buffer, 0, 3);
            OemName = new BufferByteString(buffer, 3, 8);
            VolumeLengthSectors = new BufferUInt64(buffer, 72);
            FatOffsetSector = new BufferUInt32(buffer, 80);
            FatLengthSectors = new BufferUInt32(buffer, 84);
            ClusterOffsetSector = new BufferUInt32(buffer, 88);
            ClusterCount = new BufferUInt32(buffer, 92);
            RootDirectoryCluster = new BufferUInt32(buffer, 96);
            VolumeFlags = new BufferUInt16(buffer, 106);
            BytesPerSector = new ShiftValueProvider(new BufferUInt8(buffer, 108));
            SectorsPerCluster = new ShiftValueProvider(new BufferUInt8(buffer, 109));
            NumberOfFats = new BufferUInt8(buffer, 110);
        }

        /// <summary>
        /// Reads boot sector from specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Read(Stream stream)
        {
            stream.Read(_bytes, 0, _bytes.Length);
        }
    }
}