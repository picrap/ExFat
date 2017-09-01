namespace ExFat.Core.Entries
{
    using System;

    [Flags]
    public enum ExFatDirectoryEntryType : byte
    {
        InUse = 0x80,
        IsSecondary = 0x40,
        IsBenign = 0x40,

        AllocationBitmap = 0x01,
        UpCaseTable = 0x02,
        VolumeLabel = 0x03,
        File = 0x05,

        Stream = IsSecondary,
        FileName = IsSecondary | 0x01,
    }
}