using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    internal class ParameterCollection : IParameterCollection
    {
        private readonly int _count;
        private readonly IParameterDescriptor[] _parameterEntries;

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
            _count = parameters.Length;
            _parameterEntries = new ParameterDescriptor[_count];
            for (var i = 0; i < _count; i++)
            {
                _parameterEntries[i] = new ParameterDescriptor(parameters[i], parameterInfos[i]);
            }
        }

        public IParameterDescriptor this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "index value out of range.");
                }
                return _parameterEntries[index];
            }
        }

        public IParameterDescriptor this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                var count = _count;
                var parameters = _parameterEntries;
                if (count == 1)
                {
                    var descriptor = parameters[0];
                    if (descriptor.Name == name)
                    {
                        return descriptor;
                    }
                    throw ThrowNotFound();
                }
                IParameterDescriptor parameter;
                for (var i = 0; i < count; i++)
                {
                    parameter = parameters[i];
                    if (parameters[i].Name == name)
                        return parameter;
                }
                throw ThrowNotFound();
                InvalidOperationException ThrowNotFound()
                {
                    return new InvalidOperationException($"Not found the parameter named \"{name}\".");
                }
            }
        }

        public int Count
        {
            get { return _count; }
        }

        public IEnumerator<IParameterDescriptor> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return _parameterEntries[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}