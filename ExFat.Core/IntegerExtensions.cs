// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat
{
    using System;

    /// <summary>
    /// Extensions to <see cref="int"/> and friends
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Rotates 1 bit right.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static UInt16 RotateRight(this UInt16 v)
        {
            return (UInt16)((v << 15) | (v >> 1));
        }

        /// <summary>
        /// Rotates 1 bit right.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static UInt32 RotateRight(this UInt32 v)
        {
            return (v << 31) | (v >> 1);
        }
    }
}