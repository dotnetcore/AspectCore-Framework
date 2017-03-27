using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace AspectCore.Abstractions
{
    public sealed class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> data;

        public DynamicDictionary()
        {
            data = new Dictionary<string, object>();
        }

        #region IDictionary
        public object this[string key]
        {
            get
            {
                return data[key];
            }

            set
            {
                data[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return data.Count;
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
                return data.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return data.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            data.Add(item);
        }

        public void Add(string key, object value)
        {
            data.Add(key, value);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return data.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
          return  data.Remove(item);
        }

        public bool Remove(string key)
        {
          return  data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return data.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
        #endregion

        #region DynamicObject

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentNullException(nameof(binder));
            }

            result = data[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw new ArgumentNullException(nameof(binder));
            }

            data[binder.Name] = value;

            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return data.Keys;
        }

        #endregion
    }
}
