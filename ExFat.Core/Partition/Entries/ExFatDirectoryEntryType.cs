// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;

    [Flags]
    public enum ExFatDirectoryEntryType : byte
    {
        // values

        AllocationBitmap = 0x01,
        UpCaseTable = 0x02,
        VolumeLabel = 0x03,
        File = 0x05,

        Stream = Secondary,
        FileName = Secondary | 0x01,

        // flags

        InUse = 0x80,
        Secondary = 0x40,
        Benign = 0x20,
    }
}