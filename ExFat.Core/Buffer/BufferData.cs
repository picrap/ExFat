namespace ExFat.Core.Buffer
{
    using System;
    using System.Collections.Generic;

    public abstract class BufferData
    {
        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly int _length;

        protected void SetAt(int index, byte value)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();
            _buffer[_offset + index] = value;
        }

        protected byte GetAt(int index)
        {
            if (index < 0 || index >= _length)
                throw new ArgumentOutOfRangeException();
            return _buffer[_offset + index];
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns></returns>
        protected byte[] GetAll()
        {
            var bytes = new byte[_length];
            Array.Copy(_buffer, _offset, bytes, 0, _length);
            return bytes;
        }

        protected void Set(IList<byte> bytes)
        {
            if (bytes.Count > _length)
                throw new ArgumentException(nameof(bytes));
            for (int index = 0; index < bytes.Count; index++)
                _buffer[_offset + index] = bytes[index];
        }

        protected BufferData(byte[] buffer, int offset, int length)
        {
            _buffer = buffer;
            _offset = offset;
            _length = length;
        }
    }
}