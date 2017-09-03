namespace ExFat.Core.Entries
{
    using System;
    using Buffers;
    public class EntryDateTime : IValueProvider<DateTime>
    {
        private readonly IValueProvider<ushort> _dateTimeProvider;
        private readonly IValueProvider<byte> _tenMsProvider;

        public DateTime Value
        {
            get { return DateTimeUtility.FromTimeStamp(_dateTimeProvider.Value, _tenMsProvider.Value); }
            set
            {
                var t = value.ToTimeStamp();
                _dateTimeProvider.Value = t.Item1;
                _tenMsProvider.Value = t.Item2;
            }
        }

        public EntryDateTime(IValueProvider<UInt16> dateTimeProvider, IValueProvider<Byte> tenMsProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _tenMsProvider = tenMsProvider;
        }
    }
}