using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Descriptors
{

    public sealed class ParameterCollection : IEnumerable<ParameterDescriptor>, IReadOnlyList<ParameterDescriptor>
    {
        private readonly IDictionary<string, ParameterDescriptor> parameterEntries;

        public ParameterCollection(object[] parameters, ParameterInfo[] parameterInfos)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (parameterInfos == null) throw new ArgumentNullException(nameof(parameterInfos));
            if (parameters.Length != parameterInfos.Length)
                throw new ArgumentException("the number of parameters must equal the number of parameterInfos.");

            parameterEntries = new Dictionary<string, ParameterDescriptor>(parameterInfos.Length);

            for (int index = 0; index < parameterInfos.Length; index++)
            {
                parameterEntries.Add(parameterInfos[index].Name, new ParameterDescriptor(parameters[index], parameterInfos[index]));
            }
        }

        public ParameterDescriptor this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index), "index value out of range.");
                ParameterDescriptor[] descriptors = parameterEntries.Select(pair => pair.Value).ToArray();
                return descriptors[index];
            }
        }

        public ParameterDescriptor this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
                ParameterDescriptor descriptor;
                if (parameterEntries.TryGetValue(name, out descriptor)) return descriptor;
                throw new KeyNotFoundException($"does not exist the parameter nameof \"{name}\".");
            }
        }

        public int Count
        {
            get
            {
                return parameterEntries.Count;
            }
        }

        public IEnumerator<ParameterDescriptor> GetEnumerator()
        {
            IEnumerable<ParameterDescriptor> entries = parameterEntries.Values;
            foreach (ParameterDescriptor descriptor in entries)
                yield return descriptor;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}