namespace ExFat.Core.Entries
{
    using System;

    [Flags]
    public enum AllocationBitmapFlags : Byte
    {
        SecondClusterBitmap = 0x01,
    }
}