// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 8-bit char string
    /// </summary>
    /// <seealso cref="string" />
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferByteString : IValueProvider<string>
    {
        private readonly Encoding _encoding;
        private readonly Buffer _buffer;

        private IEnumerable<byte> GetZeroBytes()
        {
            foreach (var b in _buffer.GetBytes())
            {
                if (b == 0)
                    break;
                yield return b;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return _encoding.GetString(GetZeroBytes().ToArray()); }
            set
            {
                var stringBytes = _encoding.GetBytes(value);
                // first of all, inject bytes
                _buffer.Set(stringBytes);
                // then pad
                for (int index = stringBytes.Length; index < _buffer.Length; index++)
                    _buffer[index] = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferByteString" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="encoding">The encoding (defaults to ASCII).</param>
        public BufferByteString(Buffer buffer, int offset, int length, Encoding encoding = null)
        {
            _buffer = new Buffer(buffer, offset, length);
            _encoding = encoding ?? Encoding.ASCII;
        }
    }
}