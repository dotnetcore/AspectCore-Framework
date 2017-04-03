using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class ParameterCollection : IEnumerable<ParameterDescriptor>, IReadOnlyList<ParameterDescriptor>
    {
        private readonly IDictionary<string, ParameterDescriptor> _parameterEntries;

        public ParameterCollection(object[] parameters, ParameterInfo[] parameterInfos)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (parameterInfos == null)
            {
                throw new ArgumentNullException(nameof(parameterInfos));
            }
            if (parameters.Length != parameterInfos.Length)
            {
                throw new ArgumentException("The number of parameters must equal the number of parameterInfos.");
            }

            _parameterEntries = new Dictionary<string, ParameterDescriptor>(parameterInfos.Length);

            for (int index = 0; index < parameterInfos.Length; index++)
            {
                _parameterEntries.Add(parameterInfos[index].Name, new ParameterDescriptor(parameters[index], parameterInfos[index]));
            }
        }

        public virtual ParameterDescriptor this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "index value out of range.");
                }
                ParameterDescriptor[] descriptors = _parameterEntries.Select(pair => pair.Value).ToArray();
                return descriptors[index];
            }
        }

        public virtual ParameterDescriptor this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }
                ParameterDescriptor descriptor = null;
                if (!_parameterEntries.TryGetValue(name, out descriptor))
                {
                    throw new KeyNotFoundException($"Does not exist the parameter nameof \"{name}\".");
                }
                return descriptor;
            }
        }

        public virtual int Count
        {
            get
            {
                return _parameterEntries.Count;
            }
        }

        public virtual IEnumerator<ParameterDescriptor> GetEnumerator()
        {
            IEnumerable<ParameterDescriptor> entries = _parameterEntries.Values;
            foreach (ParameterDescriptor descriptor in entries)
                yield return descriptor;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}