namespace ExFat.Core.Entries
{
    using System;

    [Flags]
    public enum ExFatDirectoryEntryType : byte
    {
        Allocated = 0x80,

        AllocationBitmap = 0x01,
        UpCaseTable = 0x02,
        VolumeLabel = 0x03,
        File = 0x05,

        Stream = 0x40,
        FileName = 0x41,
    }
}