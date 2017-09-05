// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat
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
            return (ushort) ((v << 15) | (v >> 1));
        }
    }
}