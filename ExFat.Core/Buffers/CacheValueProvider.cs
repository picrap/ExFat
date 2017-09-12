// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Buffers
{
    /// <summary>
    /// Creates a cache value provider
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="ExFat.Buffers.IValueProvider{TValue}" />
    public class CacheValueProvider<TValue> : IValueProvider<TValue>
    {
        private readonly IValueProvider<TValue> _valueProvider;

        private bool _valueSet;
        private TValue _value;

        public TValue Value
        {
            get
            {
                if (!_valueSet)
                {
                    _value = _valueProvider.Value;
                    _valueSet = true;
                }
                return _value;
            }
            set { _valueProvider.Value = _value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheValueProvider{TValue}"/> class.
        /// </summary>
        /// <param name="valueProvider">The value provider.</param>
        public CacheValueProvider(IValueProvider<TValue> valueProvider)
        {
            _valueProvider = valueProvider;
        }
    }
}