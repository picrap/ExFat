// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;

    [Flags]
    public enum ExFatGeneralSecondaryFlags : Byte
    {
        ClusterAllocationPossible = 0x01,
        NoFatChain = 0x02,
    }
}