using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    public sealed class AspectDictionary : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _data;

        public AspectDictionary()
        {
            _data = new Dictionary<string, object>();
        }

        #region IDictionary
        public object this[string key]
        {
            get
            {
                return _data[key];
            }

            set
            {
                _data[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _data.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return _data.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _data.Add(item);
        }

        public void Add(string key, object value)
        {
            _data.Add(key, value);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _data.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
          return  _data.Remove(item);
        }

        public bool Remove(string key)
        {
          return  _data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
        #endregion
    }
}
