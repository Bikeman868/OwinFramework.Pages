using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Mocks.Collections
{
    public class MockDictionaryFactory : ConcreteImplementationProvider<IDictionaryFactory>, IDictionaryFactory
    {
        protected override IDictionaryFactory GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IThreadSafeDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> equalityComparer = null)
        {
            return new MockDictionary<TKey, TValue>();
        }

        public class EnumerableLocked<T> : IEnumerableLocked<T>
        {
            private readonly IEnumerable<T> _enumerable;

            public EnumerableLocked(IEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }

            public void Dispose()
            {
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class MockDictionary<TKey, TValue> : IThreadSafeDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> createValueFunction, Action<TKey, TValue> initializeAction)
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];

                var value = createValueFunction(key);
                _dictionary[key] = value;

                if (initializeAction != null)
                    initializeAction(key, value);

                return value;
            }

            public IEnumerableLocked<TKey> KeysLocked
            {
                get { return new EnumerableLocked<TKey>(_dictionary.Keys); }
            }

            public IEnumerableLocked<TValue> ValuesLocked
            {
                get { return new EnumerableLocked<TValue>(_dictionary.Values); }
            }

            public IEnumerableLocked<KeyValuePair<TKey, TValue>> KeyValuePairsLocked
            {
                get { return new EnumerableLocked<KeyValuePair<TKey, TValue>>(_dictionary); }
            }

            public TKey GetNextKey(ref int index)
            {
                if (index >= _dictionary.Count)
                    index = 0;

                var key = _dictionary.Keys.Skip(index).First();
                index++;

                return key;
            }

            public TValue GetNextValue(ref int index)
            {
                if (index >= _dictionary.Count)
                    index = 0;

                var value = _dictionary.Values.Skip(index).First();
                index++;

                return value;
            }

            public TValue Update(TKey key, Func<TValue, TValue> updateMethod)
            {
                return _dictionary[key] = updateMethod(_dictionary[key]);
            }

            public void Add(TKey key, TValue value)
            {
                _dictionary[key] = value;
            }

            public bool ContainsKey(TKey key)
            {
                return _dictionary.ContainsKey(key);
            }

            public ICollection<TKey> Keys
            {
                get { return _dictionary.Keys; }
            }

            public bool Remove(TKey key)
            {
                return _dictionary.Remove(key);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public ICollection<TValue> Values
            {
                get { return _dictionary.Values; }
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                _dictionary.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return _dictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                (_dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false ; }
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return (_dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }
        }
    }
}
