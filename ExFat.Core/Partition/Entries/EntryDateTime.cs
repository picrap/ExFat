// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition.Entries
{
    using System;
    using Buffers;

    public class EntryDateTime : IValueProvider<DateTime>
    {
        private readonly IValueProvider<UInt32> _dateTimeProvider;
        private readonly IValueProvider<Byte> _tenMsProvider;

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

        public EntryDateTime(IValueProvider<UInt32> dateTimeProvider, IValueProvider<Byte> tenMsProvider = null)
        {
            _dateTimeProvider = dateTimeProvider;
            _tenMsProvider = tenMsProvider;
        }
    }
}