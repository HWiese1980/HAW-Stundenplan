using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW.Depending
{
    class ThreadSafeObservableDictionary<T1, T2> : IDictionary<T1, T2>, INotifyCollectionChanged
    {
        private readonly Dictionary<T1, T2> _innerDictionary = new Dictionary<T1, T2>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<T1,T2>>

        public void Add(KeyValuePair<T1, T2> item)
        {
            _innerDictionary.Add(item.Key, item.Value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }));
        }

        public void Clear()
        {
            _innerDictionary.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<T1, T2> item)
        {
            return _innerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            _innerDictionary.ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<T1, T2> item)
        {
            var b = _innerDictionary.Remove(item.Key);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }));
            return b;
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IDictionary<T1,T2>

        public bool ContainsKey(T1 key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void Add(T1 key, T2 value)
        {
            _innerDictionary.Add(key, value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { new KeyValuePair<T1, T2>(key, value) }));
        }

        public bool Remove(T1 key)
        {
            var i = _innerDictionary[key];
            var b = _innerDictionary.Remove(key);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { i }));
            return b;
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        public T2 this[T1 key]
        {
            get { return _innerDictionary[key]; }
            set
            {
                var old = _innerDictionary.ContainsKey(key) ? _innerDictionary[key] : default(T2);
                _innerDictionary[key] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { value }, new[] { old }));
            }
        }

        public ICollection<T1> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public ICollection<T2> Values
        {
            get { return _innerDictionary.Values; }
        }

        #endregion
    }
}
