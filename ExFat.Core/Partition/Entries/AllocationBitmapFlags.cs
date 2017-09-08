// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;

    /// <summary>
    /// Flags for <see cref="ExFatAllocationBitmap"/>
    /// </summary>
    [Flags]
    public enum AllocationBitmapFlags : Byte
    {
        /// <summary>
        /// Indicates this bitmap is the second allocation bitmap
        /// </summary>
        SecondClusterBitmap = 0x01,
    }
}