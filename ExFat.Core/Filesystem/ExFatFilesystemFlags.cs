// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;

    /// <summary>
    /// Partition management flags
    /// </summary>
    [Flags]
    public enum ExFatFilesystemFlags
    {
        /// <summary>
        /// When set, all reads update the last access time
        /// </summary>
        UpdateLastAccessTime = 0x0001,

        /// <summary>
        /// Default value
        /// </summary>
        Default = UpdateLastAccessTime,
    }
}