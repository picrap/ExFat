// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;

    [Flags]
    public enum AllocationBitmapFlags : Byte
    {
        SecondClusterBitmap = 0x01,
    }
}