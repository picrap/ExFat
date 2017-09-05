// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;

    [Flags]
    public enum ExFatFilesystemFlags
    {
        UpdateLastAccessTime = 0x0001,

        Default = UpdateLastAccessTime,
    }
}