// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using Buffers;

    /// <summary>
    /// Allows to specify time zones
    /// </summary>
    /// <seealso cref="TimeZoneInfo" />
    public class EntryTimeZone : IValueProvider<TimeZoneInfo>
    {
        private readonly IValueProvider<Byte> _timeZoneOffsetProvider;

        public TimeZoneInfo Value
        {
            get { return DateTimeUtility.FromTimeZoneOffset(_timeZoneOffsetProvider.Value); }
            set { _timeZoneOffsetProvider.Value = value.ToTimeZoneOffset(); }
        }

        public EntryTimeZone(IValueProvider<Byte> timeZoneOffsetProvider)
        {
            _timeZoneOffsetProvider = timeZoneOffsetProvider;
        }
    }
}