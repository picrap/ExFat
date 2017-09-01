namespace ExFat.Core.Entries
{
    using System;

    [Flags]
    public enum ExFatGeneralSecondaryFlags : Byte
    {
        ClusterAllocationPossible = 0x01,
        NoFatChain = 0x02,
    }
}