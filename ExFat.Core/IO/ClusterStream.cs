namespace ExFat.Core.IO
{
    using System;
    using System.IO;
    public class ClusterStream : Stream
    {
        private readonly IClusterReader _clusterReader;
        private readonly IClusterWriter _clusterWriter;
        private readonly long _startCluster;
        private bool _contiguous;
        private readonly Action _onDisposed;
        private long? _length;
        private long _position;

        private byte[] _currentClusterBuffer;
        private long _currentClusterDataIndex = -1;
        private long CurrentClusterIndex => _position / _clusterReader.BytesPerCluster;
        private long _previousCluster, _currentCluster;
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

            _position = 0;
            _previousCluster = 0;
            _currentCluster = _startCluster;
        }

        protected override void Dispose(bool disposing)
        {
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
            if (_currentClusterDirty)
            {
                _clusterWriter.WriteCluster(_currentCluster, _currentClusterBuffer);
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

            // simplify complex seeks
            if (origin == SeekOrigin.End)
                return Seek(_length.Value + offset, SeekOrigin.Begin);
            if (origin == SeekOrigin.Current)
                return Seek(_position + offset, SeekOrigin.Begin);

            if (offset < 0)
                offset = 0;
            if (offset > _length)
                offset = _length.Value;

            // if the seek is in the same cluster, keep here
            var clusterIndex = offset / _clusterReader.BytesPerCluster;
            if (clusterIndex == CurrentClusterIndex)
            {
                _position = offset;
                return _position;
            }

            // seek to begin is also special, we clear it all
            if (offset == 0)
            {
                _position = 0;
                _previousCluster = 0;
                _currentCluster = _startCluster;
                return 0;
            }

            // if we need to rewind, get forward from scratch
            if (offset < _position)
            {
                Seek(0, SeekOrigin.Begin);
                return Seek(offset, SeekOrigin.Begin);
            }

            // now, it's only a forward seek
            SeekNextCluster(clusterIndex - CurrentClusterIndex);
            _position = offset;
            return offset;
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the current cluster.
        /// </summary>
        /// <returns></returns>
        private byte[] GetCurrentCluster()
        {
            // lazy initialization of cluster index
            if (_currentClusterBuffer == null)
                _currentClusterBuffer = new byte[_clusterReader.BytesPerCluster];

            // if the current requested cluster is not the one we have, read it
            // we get here after to operations:
            // 1. (most common) read next cluster after current is exhausted (Read() has reached its buffer end)
            // 2. after a Seek()
            if (CurrentClusterIndex != _currentClusterDataIndex)
            {
                // end of stream
                if (_currentCluster < 0)
                {
                    if (!CanWrite)
                        return null;

                    // there is probably a pending cluster
                    Flush();

                    // allocate new cluster
                    var newCluster = _clusterWriter.AllocateCluster(_previousCluster);
                    _clusterWriter.SetNextCluster(_previousCluster, newCluster);

                    // from contiguous to sparse mode, make sure all clusters are linked
                    if (_contiguous && newCluster != _previousCluster + 1)
                    {
                        _contiguous = false;
                        for (int clusterIndex = 0; clusterIndex < CurrentClusterIndex; clusterIndex++)
                            _clusterWriter.SetNextCluster(_startCluster + clusterIndex, _startCluster + clusterIndex + 1);
                    }

                    _previousCluster = newCluster;
                    _currentClusterDirty = true;
                    // give something clean
                    Array.Clear(_currentClusterBuffer, 0, _currentClusterBuffer.Length);
                    _currentClusterDataIndex++;
                    return _currentClusterBuffer;
                }

                // optionnally flushes a pending change
                if (CanWrite)
                    Flush();
                _clusterReader.ReadCluster(_currentCluster, _currentClusterBuffer);
                _currentClusterDataIndex = CurrentClusterIndex;
                SeekNextCluster(1);
            }

            return _currentClusterBuffer;
        }

        private long GetNextCluster(long cluster, long clustersCount, out long previousCluster)
        {
            if (clustersCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(cluster), "cluster must be greater than 0");

            if (_contiguous)
            {
                previousCluster = cluster + clustersCount - 1;
                return cluster + clustersCount;
            }

            previousCluster = cluster;
            for (var index = 0; index < clustersCount; index++)
            {
                previousCluster = cluster;
                cluster = _clusterReader.GetNextCluster(cluster);
            }
            return cluster;
        }

        private void SeekNextCluster(long clustersCount)
        {
            _currentCluster = GetNextCluster(_currentCluster, clustersCount, out _previousCluster);
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
                var currentCluster = GetCurrentCluster();
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
                var currentCluster = GetCurrentCluster();
                Buffer.BlockCopy(buffer, offset, currentCluster, CurrentClusterOffset, toWrite);
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