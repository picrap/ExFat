// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat
{
    using System.Collections;
    using System.Collections.Generic;

    public class Cache<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private readonly IList<TKey> _orderedKeys = new List<TKey>();

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;

        public TValue this[TKey key]
        {
            get
            {
                var value = _dictionary[key];
                Touch(key);
                return value;
            }
            set
            {
                _dictionary[key] = value;
                Touch(key);
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;

        public Cache(int capacity)
        {
            _capacity = capacity;
        }

        public void Touch(TKey key)
        {
            // remove from anywhere
            _orderedKeys.Remove(key);
            // place at end
            _orderedKeys.Add(key);
            // on capacity overflow
            while (_orderedKeys.Count >= _capacity)
            {
                // oldest key is first
                var lastKey = _orderedKeys[0];
                _orderedKeys.RemoveAt(0);
                _dictionary.Remove(lastKey);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
            Touch(item.Key);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _orderedKeys.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Remove(item);
            return _orderedKeys.Remove(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            Touch(key);
        }

        public bool Remove(TKey key)
        {
            _dictionary.Remove(key);
            return _orderedKeys.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_dictionary.TryGetValue(key, out value))
                return false;
            Touch(key);
            return true;
        }
    }
}