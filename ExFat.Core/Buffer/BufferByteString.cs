namespace ExFat.Core.Buffer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    [DebuggerDisplay("{Value}")]
    public class BufferByteString : BufferData
    {
        private readonly Encoding _encoding;

        private IEnumerable<byte> GetZero()
        {
            foreach (var b in GetAll())
            {
                if (b == 0)
                    break;
                yield return b;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return _encoding.GetString(GetZero().ToArray()); }
            set { Set(_encoding.GetBytes(value)); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferByteString"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="encoding">The encoding (defaults to ASCII).</param>
        public BufferByteString(byte[] buffer, int offset, int length, Encoding encoding = null)
            : base(buffer, offset, length)
        {
            _encoding = encoding ?? Encoding.ASCII;
        }
    }
}