// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// 16-bit char string buffer
    /// </summary>
    /// <seealso cref="string" />
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class BufferWideString : IValueProvider<string>
    {
        private readonly Buffer _buffer;

        private IEnumerable<char> GetChars()
        {
            var all = _buffer.GetBytes();
            for (int index = 0; index < all.Length; index += 2)
                yield return ToChar(all[index], all[index + 1]);
        }

        private IEnumerable<char> GetZeroChars()
        {
            foreach (var b in GetChars())
            {
                if (b == 0)
                    break;
                yield return b;
            }
        }

        private static char ToChar(byte first, byte second)
        {
            return (char)(first | second << 8);
        }

        private static byte[] ToBytes(char c)
        {
            return new[] { (byte)(c & 0xFF), (byte)(c >> 8) };
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
            get { return new string(GetZeroChars().ToArray()); }
            set
            {
                for (int byteIndex = 0, charIndex = 0; byteIndex < _buffer.Length; byteIndex += 2, charIndex++)
                {
                    if (charIndex < value.Length)
                    {
                        var t = ToBytes(value[charIndex]);
                        _buffer[byteIndex] = t[0];
                        _buffer[byteIndex + 1] = t[1];
                    }
                    else
                    {
                        _buffer[byteIndex] = 0;
                        _buffer[byteIndex + 1] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferByteString" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="charsLength">The length.</param>
        public BufferWideString(Buffer buffer, int offset, int charsLength)
        {
            _buffer = new Buffer(buffer, offset, charsLength * 2);
        }
    }
}