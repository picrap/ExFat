namespace ExFat.Core.Entries
{
    using System.Text;
    using Buffers;

    /// <summary>
    /// Directory entry in exFAT
    /// Thanks to https://events.linuxfoundation.org/images/stories/pdf/lceu11_munegowda_s.pdf
    /// </summary>
    public class FatDirectoryEntry
    {
        public BufferByteString DirName { get; }
        public BufferUInt8 DirAttr { get; }
        public BufferUInt8 DirNtRes { get; }
        public BufferUInt8 DirCrtTimeTenth { get; }
        public BufferUInt16 DirCrtTime { get; }
        public BufferUInt16 DirCrtDate { get; }
        public BufferUInt16 DirLastAccDate { get; }
        public BufferUInt16 DirFstClusHI { get; }
        public BufferUInt16 DirWrtTime { get; }
        public BufferUInt16 DirWrtDate { get; }
        public BufferUInt16 DirFstClusLO { get; }
        public BufferUInt32 DirFileSize { get; }

        public FatDirectoryEntry(Buffer buffer)
        {
            DirName = new BufferByteString(buffer, 0, 11, Encoding.Default);
            DirAttr = new BufferUInt8(buffer, 11);
            DirNtRes = new BufferUInt8(buffer, 12);
            DirCrtTimeTenth = new BufferUInt8(buffer, 13);
            DirCrtTime = new BufferUInt16(buffer, 14);
            DirCrtDate = new BufferUInt16(buffer, 16);
            DirLastAccDate = new BufferUInt16(buffer, 18);
            DirFstClusHI = new BufferUInt16(buffer, 20);
            DirWrtTime = new BufferUInt16(buffer, 22);
            DirWrtDate = new BufferUInt16(buffer, 24);
            DirFstClusLO = new BufferUInt16(buffer, 26);
            DirFileSize = new BufferUInt32(buffer, 28);
        }
    }
}
