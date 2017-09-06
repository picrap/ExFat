// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using Buffers;

    public class EntryDateTimeOffset : IValueProvider<DateTimeOffset>
    {
        private readonly IValueProvider<DateTime> _dateTimeProvider;
        private readonly IValueProvider<TimeSpan> _offsetProvider;

        public DateTimeOffset Value
        {
            get { return new DateTimeOffset(_dateTimeProvider.Value, _offsetProvider.Value); }
            set
            {
                _dateTimeProvider.Value = value.DateTime;
                _offsetProvider.Value = value.Offset;
            }
        }

        public EntryDateTimeOffset(IValueProvider<DateTime> dateTimeProvider, IValueProvider<TimeSpan> offsetProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _offsetProvider = offsetProvider;
        }
    }
}