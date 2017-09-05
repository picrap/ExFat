// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Filesystem
{
    using System;
    using System.IO;
    using Partition;

    public class ExFatFilesystem : IDisposable
    {
        private readonly ExFatPartition _partition;

        public ExFatFilesystem(Stream partitionStream)
        {
            _partition = new ExFatPartition(partitionStream);
        }

        public void Dispose()
        {
            _partition.Dispose();
        }
    }
}