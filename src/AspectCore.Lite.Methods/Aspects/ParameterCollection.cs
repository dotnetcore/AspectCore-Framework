using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static AspectCore.Lite.Methods.Aspects.ParameterCollection;

namespace AspectCore.Lite.Methods.Aspects
{

    public sealed class ParameterCollection : IEnumerable<ParameterEntry>, IReadOnlyCollection<ParameterEntry>
    {
        private readonly Lazy<IList<ParameterEntry>> parameterList;

        public ParameterCollection(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                parameterList = new Lazy<IList<ParameterEntry>>();
            }
            else
            {
                parameterList = new Lazy<IList<ParameterEntry>>(() => new List<ParameterEntry>(parameters.Count));
                foreach (var parameter in parameters)
                    parameterList.Value.Add(new ParameterEntry(parameter.Key, parameter.Value));
            }
        }

        public object this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                ParameterEntry entry = (parameterList.Value.FirstOrDefault(e => e.Name == name));
                if (entry.Name != null) return entry.Value;
                throw new MissingMemberException($"does not exist the parameter nameof {name}.");
            }
            set
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                ParameterEntry entry = (parameterList.Value.FirstOrDefault(e => e.Name == name));
                if (entry.Name != null) entry.Value = value;
                throw new MissingMemberException($"does not exist the parameter nameof {name}.");
            }
        }

        public int Count
        {
            get
            {
                return parameterList.IsValueCreated ? parameterList.Value.Count : 0;
            }
        }

        public IEnumerator<ParameterEntry> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<ParameterEntry>, IEnumerator
        {
            private readonly ParameterCollection collection;
            private ParameterEntry current;
            private int index;

            internal Enumerator(ParameterCollection collection)
            {
                this.collection = collection;
                index = 0;
                current = new ParameterEntry();
            }

            public ParameterEntry Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == collection.Count + 1))
                    {
                        throw new InvalidOperationException("enum value is out of range");
                    }
                    return new ParameterEntry(current.Name, current.Value);
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                while ((uint)index < (uint)collection.Count)
                {
                    current = new ParameterEntry(collection.parameterList.Value[index].Name, collection.parameterList.Value[index].Value);
                    index++;
                    return true;
                }

                index = collection.Count + 1;
                current = new ParameterEntry();
                return false;
            }

            public void Reset()
            {
                index = 0;
                current = new ParameterEntry();
            }
        }
    }

    public struct ParameterEntry
    {
        private string _name;
        private object _value;
        public ParameterEntry(string name, object value)
        {
            _name = name;
            _value = value;
        }
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }
        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }
}
