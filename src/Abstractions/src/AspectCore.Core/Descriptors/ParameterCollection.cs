using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    internal class ParameterCollection : IParameterCollection
    {
        private readonly Lazy<Dictionary<string, IParameterDescriptor>> _parameterEntries;

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

            _parameterEntries = new Lazy<Dictionary<string, IParameterDescriptor>>(() =>
            {
                var dic = new Dictionary<string, IParameterDescriptor>(parameterInfos.Length);
                for (int index = 0; index < parameterInfos.Length; index++)
                {
                    dic.Add(parameterInfos[index].Name, new ParameterDescriptor(parameters[index], parameterInfos[index]));
                }
                return dic;
            });
        }

        public IParameterDescriptor this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "index value out of range.");
                }
                var descriptors = _parameterEntries.Value.Select(pair => pair.Value).ToArray();
                return descriptors[index];
            }
        }

        public IParameterDescriptor this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }
                if (!_parameterEntries.Value.TryGetValue(name, out IParameterDescriptor descriptor))
                {
                    throw new KeyNotFoundException($"Does not exist the parameter nameof \"{name}\".");
                }
                return descriptor;
            }
        }

        public int Count
        {
            get
            {
                return _parameterEntries.Value.Count;
            }
        }

        public IEnumerator<IParameterDescriptor> GetEnumerator()
        {
            return _parameterEntries.Value.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}