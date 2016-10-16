using AspectCore.Lite.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class ParameterCollection : IEnumerable<ParameterDescriptor>, IReadOnlyList<ParameterDescriptor>
    {
        private readonly IDictionary<string, ParameterDescriptor> parameterEntries;

        public ParameterCollection(object[] parameters, ParameterInfo[] parameterInfos)
        {
            ExceptionUtilities.ThrowArgumentNull(parameters , nameof(parameters));
            ExceptionUtilities.ThrowArgumentNull(parameterInfos , nameof(parameterInfos));
            ExceptionUtilities.ThrowArgument(() => parameters.Length != parameterInfos.Length , "The number of parameters must equal the number of parameterInfos.");

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
                ExceptionUtilities.Throw<ArgumentOutOfRangeException>(() => index < 0 || index >= Count , nameof(index) , "index value out of range.");
                ParameterDescriptor[] descriptors = parameterEntries.Select(pair => pair.Value).ToArray();
                return descriptors[index];
            }
        }

        public ParameterDescriptor this[string name]
        {
            get
            {
                ExceptionUtilities.ThrowArgumentNullOrEmpty(name , nameof(name));
                ParameterDescriptor descriptor = null;
                ExceptionUtilities.Throw<KeyNotFoundException>(() => !parameterEntries.TryGetValue(name , out descriptor) , $"Does not exist the parameter nameof \"{name}\".");
                return descriptor;
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