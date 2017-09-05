// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.IO
{
    using System;
    using System.IO;
    using Partition;

    public class ClusterStream : PartitionStream
    {
        private readonly IClusterReader _clusterReader;
        private readonly IClusterWriter _clusterWriter;
        private readonly long _startCluster;
        private readonly long? _lastContiguousCluster; // filled for contiguous files
        private bool _contiguous;
        private readonly Action _onDisposed;
        private long? _length;
        private long _position;

        private byte[] _currentClusterBuffer;
        private long _currentClusterDataIndex = -1;
        private long CurrentClusterIndexFromPosition => _position / _clusterReader.BytesPerCluster;
        private long _currentCluster;
        private bool _currentClusterDirty;

        private int CurrentClusterOffset => (int)_position % _clusterReader.BytesPerCluster;

        public override bool CanRead => true;
        public override bool CanSeek => _length.HasValue;
        public override bool CanWrite => _clusterWriter != null;

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public override long Length
        {
            get
            {
                if (!_length.HasValue)
                    throw new NotSupportedException();
                return _length.Value;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { return Seek(0, SeekOrigin.Current); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override long ClusterPosition
        {
            get
            {
                // TODO: something less lazy, because this may consume resources
                // invoking GetCurrentCluster with readonly mode will allow to actually seek current cluster
                // unwanted side-effect: it may read it (which is harmless, but may be useless)
                GetCurrentCluster(false);
                if (CurrentClusterIndexFromPosition == _currentClusterDataIndex)
                    return _currentCluster;
                return -1;
            }
        }

        public override int ClusterOffset => CurrentClusterOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterStream" /> class.
        /// </summary>
        /// <param name="clusterReader">The cluster information reader.</param>
        /// <param name="clusterWriter">The cluster writer.</param>
        /// <param name="startCluster">The start cluster.</param>
        /// <param name="contiguous">if set to <c>true</c> [contiguous].</param>
        /// <param name="length">The length.</param>
        /// <param name="onDisposed">The on disposed.</param>
        /// <exception cref="ArgumentException">If contiguous is true, the length must be specified</exception>
        public ClusterStream(IClusterReader clusterReader, IClusterWriter clusterWriter, ulong startCluster, bool contiguous, ulong? length, Action onDisposed)
        {
            if (contiguous && !length.HasValue)
                throw new ArgumentException("If contiguous is true, the length must be specified");

            _clusterReader = clusterReader;
            _clusterWriter = clusterWriter;
            _startCluster = (long)startCluster;
            _contiguous = contiguous;
            _onDisposed = onDisposed;
            _length = (long?)length;
            if (contiguous && _length.HasValue)
                _lastContiguousCluster = _startCluster + (_length.Value + _clusterReader.BytesPerCluster - 1) / _clusterReader.BytesPerCluster - 1;

            _position = 0;
            _currentCluster = _startCluster;
        }

        protected override void Dispose(bool disposing)
        {
            FlushCurrentCluster();
            base.Dispose(disposing);
            if (disposing && _onDisposed != null)
                _onDisposed();
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if (!CanWrite)
                throw new NotSupportedException();

            FlushCurrentCluster();
        }

        private void FlushCurrentCluster()
        {
            if (_currentClusterDirty)
            {
                _clusterWriter.WriteCluster(_currentCluster, _currentClusterBuffer, 0, _currentClusterBuffer.Length);
                _currentClusterDirty = false;
            }
        }

        /// <summary>Sets the position within the current stream.</summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_length.HasValue)
                throw new InvalidOperationException();

            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = _length.Value + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (newPosition < 0)
                newPosition = 0;
            if (newPosition > _length)
                newPosition = _length.Value;

            _position = newPosition;
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current cluster.
        /// </summary>
        /// <returns></returns>
        private byte[] GetCurrentCluster(bool append)
        {
            // lazy initialization of cluster index
            if (_currentClusterBuffer == null)
                _currentClusterBuffer = new byte[_clusterReader.BytesPerCluster];

            // if the current requested cluster is not the one we have, read it
            // we get here after to operations:
            // 1. (most common) read next cluster after current is exhausted (Read() has reached its buffer end)
            // 2. after a Seek()
            if (CurrentClusterIndexFromPosition != _currentClusterDataIndex)
            {
                var currentCluster = GetCurrentClusterFromPosition();
                // end of stream
                if (currentCluster < 0)
                {
                    if (!CanWrite || !append)
                        return null;

                    // there is probably a pending cluster
                    FlushCurrentCluster();

                    // allocate new cluster
                    var newCluster = _clusterWriter.AllocateCluster(_currentCluster);
                    _clusterWriter.SetNextCluster(_currentCluster, newCluster);
                    _clusterWriter.SetNextCluster(newCluster, -1);

                    // from contiguous to sparse mode, make sure all clusters are linked
                    if (_contiguous && newCluster != _currentCluster + 1)
                    {
                        _contiguous = false;
                        for (int clusterIndex = 1; clusterIndex < CurrentClusterIndexFromPosition; clusterIndex++)
                            _clusterWriter.SetNextCluster(_startCluster + clusterIndex - 1, _startCluster + clusterIndex);
                    }

                    _currentCluster = newCluster;
                    _currentClusterDirty = true;
                    // give something clean
                    Array.Clear(_currentClusterBuffer, 0, _currentClusterBuffer.Length);
                    _currentClusterDataIndex = CurrentClusterIndexFromPosition;
                    return _currentClusterBuffer;
                }

                // optionnally flushes a pending change
                FlushCurrentCluster();

                _clusterReader.ReadCluster(currentCluster, _currentClusterBuffer, 0, _currentClusterBuffer.Length);
                _currentClusterDataIndex = CurrentClusterIndexFromPosition;
                _currentCluster = currentCluster;
            }

            return _currentClusterBuffer;
        }

        private long GetCurrentClusterFromPosition()
        {
            var currentClusterIndexFromPosition = CurrentClusterIndexFromPosition;
            if (currentClusterIndexFromPosition == 0)
                return _startCluster;
            // -1 means buffer is new
            if (_currentClusterDataIndex == -1)
                return GetNextCluster(_startCluster, currentClusterIndexFromPosition);
            if (currentClusterIndexFromPosition > _currentClusterDataIndex)
                return GetNextCluster(_currentCluster, currentClusterIndexFromPosition - _currentClusterDataIndex);
            return GetNextCluster(_startCluster, currentClusterIndexFromPosition);
        }

        private long GetNextCluster(long cluster, long clustersCount)
        {
            if (clustersCount < 0)
                throw new ArgumentOutOfRangeException(nameof(cluster), "cluster must be >= 0");

            if (_contiguous)
            {
                var nextCluster = cluster + clustersCount;
                if (nextCluster <= _lastContiguousCluster)
                    return nextCluster;
                return -1;
            }

            for (var index = 0; index < clustersCount && cluster >= 0; index++)
                cluster = _clusterReader.GetNextCluster(cluster);
            return cluster;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalRead = 0;
            while (count > 0)
            {
                // what remaings in current cluster
                var remainingInCluster = _clusterReader.BytesPerCluster - CurrentClusterOffset;
                var toRead = Math.Min(remainingInCluster, count);
                if (_length.HasValue)
                {
                    var leftInFile = _length.Value - _position;
                    if (leftInFile == 0)
                        break;
                    if (toRead > leftInFile)
                        toRead = (int)leftInFile;
                }
                var currentCluster = GetCurrentCluster(false);
                // null means nothing left to read (for streams without length; with length we've exited before)
                if (currentCluster == null)
                    break;
                Buffer.BlockCopy(currentCluster, CurrentClusterOffset, buffer, offset, toRead);
                _position += toRead;
                offset += toRead;
                count -= toRead;
                totalRead += toRead;
            }
            return totalRead;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="NotSupportedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException();

            while (count > 0)
            {
                // write is limited by what remaings in current cluster
                var remainingInCluster = _clusterReader.BytesPerCluster - CurrentClusterOffset;
                var toWrite = Math.Min(remainingInCluster, count);
                var currentCluster = GetCurrentCluster(true);
                Buffer.BlockCopy(buffer, offset, currentCluster, CurrentClusterOffset, toWrite);
                _currentClusterDirty = true;
                _position += toWrite;
                // pushing the limits!
                if (_position > _length)
                    _length = _position;
                offset += toWrite;
                count -= toWrite;
            }
        }
    }
}