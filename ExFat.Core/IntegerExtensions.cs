namespace ExFat.Core
{
    using System;

    public static class IntegerExtensions
    {
        /// <summary>
        /// Rotates 1 bit right.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static UInt16 RotateRight(this UInt16 v)
        {
            return (ushort)((v << 15) | (v >> 1));
        }
    }
}