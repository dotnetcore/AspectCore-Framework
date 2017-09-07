using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy.Parameters
{
    public sealed class ParameterCollection : IEnumerable<Parameter>, IReadOnlyList<Parameter>
    {
        private static readonly object[] emptyValues = new object[0];
        private readonly int _count;
        private readonly Parameter[] _parameterEntries;

        internal ParameterCollection(Parameter[] parameters)
        {
            _count = parameters.Length;
            _parameterEntries = parameters;
        }

        public Parameter this[int index]
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

        public Parameter this[string name]
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
                Parameter parameter;
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

        public IEnumerator<Parameter> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return _parameterEntries[i];
            }
        }

        public object[] GetValues()
        {
            if (_count == 0)
            {
                return emptyValues;
            }
            var values = new object[_count];
            for (var i = 0; i < _count; i++)
            {
                values[i] = _parameterEntries[i].Value;
            }
            return values;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}