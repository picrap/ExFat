namespace ExFat.Core.IO
{
    using System;
    using System.IO;
    public class ClusterStream : Stream
    {
        private readonly IClusterReader _clusterReader;
        private readonly IPartitionReader _partitionReader;
        private readonly long _startCluster;
        private readonly long? _length;
        private long _position;
        private byte[] _currentClusterData;
        private long _currentClusterDataIndex = -1;
        private long CurrentClusterIndex => _position / _partitionReader.BytesPerCluster;
        private long _currentCluster;

        private int CurrentClusterOffset => (int)_position % _partitionReader.BytesPerCluster;

        public override bool CanRead => true;
        public override bool CanSeek => _length.HasValue;
        public override bool CanWrite => false;

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
        /// <param name="partitionReader">The partition reader.</param>
        /// <param name="startCluster">The start cluster.</param>
        /// <param name="length">The length.</param>
        public ClusterStream(IClusterReader clusterReader, IPartitionReader partitionReader, long startCluster, long? length)
        {
            _clusterReader = clusterReader;
            _partitionReader = partitionReader;
            _startCluster = startCluster;
            _length = length;

            _position = 0;
            _currentCluster = _startCluster;
        }

        public override void Flush()
        {
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
            var clusterIndex = offset / _partitionReader.BytesPerCluster;
            if (clusterIndex == CurrentClusterIndex)
            {
                _position = offset;
                return _position;
            }

            // seek to begin is also special, we clear it all
            if (offset == 0)
            {
                _position = 0;
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
            for (var index = CurrentClusterIndex; index < clusterIndex; index++)
                _currentCluster = _clusterReader.GetNext(_currentCluster);
            _position = offset;
            return offset;
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        private byte[] GetCurrentCluster()
        {
            // lazy initialization of cluster index
            if (_currentClusterData == null)
                _currentClusterData = new byte[_partitionReader.BytesPerCluster];

            // if the current requested cluster is not the one we have, read it
            // we get here after to operations:
            // 1. (most common) read next cluster after current is exhausted (Read() has reached its buffer end)
            // 2. after a Seek()
            if (CurrentClusterIndex != _currentClusterDataIndex)
            {
                if (_currentCluster < 0)
                    return null;

                _partitionReader.ReadCluster(_currentCluster, _currentClusterData);
                _currentClusterDataIndex = CurrentClusterIndex;
                _currentCluster = _clusterReader.GetNext(_currentCluster);
            }

            return _currentClusterData;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalRead = 0;
            while (count > 0)
            {
                // what remaings in current cluster
                var remainingInCluster = _partitionReader.BytesPerCluster - CurrentClusterOffset;
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}