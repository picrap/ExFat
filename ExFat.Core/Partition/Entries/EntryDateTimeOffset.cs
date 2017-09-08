// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using Buffers;

    /// <inheritdoc />
    /// <summary>
    /// Provides <see cref="T:System.DateTimeOffset" /> based on <see cref="T:System.DateTime" /> and <see cref="T:System.TimeSpan" /> sources
    /// </summary>
    /// <seealso cref="DateTimeOffset" />
    public class EntryDateTimeOffset : IValueProvider<DateTimeOffset>
    {
        private readonly IValueProvider<DateTime> _dateTimeProvider;
        private readonly IValueProvider<TimeSpan> _offsetProvider;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public DateTimeOffset Value
        {
            get { return new DateTimeOffset(_dateTimeProvider.Value, _offsetProvider.Value); }
            set
            {
                _dateTimeProvider.Value = value.DateTime;
                _offsetProvider.Value = value.Offset;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDateTimeOffset"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider.</param>
        /// <param name="offsetProvider">The offset provider.</param>
        public EntryDateTimeOffset(IValueProvider<DateTime> dateTimeProvider, IValueProvider<TimeSpan> offsetProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _offsetProvider = offsetProvider;
        }
    }
}