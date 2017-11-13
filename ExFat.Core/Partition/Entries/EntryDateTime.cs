// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using Buffers;

    /// <inheritdoc />
    /// <summary>
    /// Provides <see cref="T:System.DateTime" /> based on DOS <see cref="T:System.UInt32" /> data
    /// </summary>
    /// <seealso cref="T:System.DateTime" />
    public class EntryDateTime : IValueProvider<DateTime>
    {
        private readonly IValueProvider<UInt32> _dateTimeProvider;
        private readonly IValueProvider<Byte> _tenMsProvider;

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public DateTime Value
        {
            get { return DateTimeUtility.FromTimeStamp(_dateTimeProvider.Value, _tenMsProvider != null ? _tenMsProvider.Value : (byte) 0); }
            set
            {
                var t = value.ToTimeStamp();
                _dateTimeProvider.Value = t.Item1;
                if (_tenMsProvider != null)
                    _tenMsProvider.Value = t.Item2;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDateTime"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider.</param>
        /// <param name="tenMsProvider">The ten ms provider.</param>
        public EntryDateTime(IValueProvider<UInt32> dateTimeProvider, IValueProvider<Byte> tenMsProvider = null)
        {
            _dateTimeProvider = dateTimeProvider;
            _tenMsProvider = tenMsProvider;
        }
    }
}
