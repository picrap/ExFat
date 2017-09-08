// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    using System;

    /// <summary>
    /// Converts a log2 value to its integer representation
    /// </summary>
    /// <seealso cref="uint" />
    public class ShiftValueProvider : IValueProvider<UInt32>
    {
        private readonly IValueProvider<byte> _shift;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="System.ArgumentException">value must be a power of 2</exception>
        public UInt32 Value
        {
            get { return 1u << _shift.Value; }
            set
            {
                var log2 = Math.Log(value) / Math.Log(2);
                var b = (byte) log2;
                if (log2 != b)
                    throw new ArgumentException("value must be a power of 2");
                _shift.Value = b;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftValueProvider"/> class.
        /// </summary>
        /// <param name="shift">The shift.</param>
        public ShiftValueProvider(IValueProvider<byte> shift)
        {
            _shift = shift;
        }
    }
}