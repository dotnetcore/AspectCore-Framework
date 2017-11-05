using System;
using System.Collections.Concurrent;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.RedisProfiler
{
    class PropertyReader
    {
        private readonly ConcurrentDictionary<string, PropertyReflector> _reflectorCache;
        private readonly Type _instanceType;

        public PropertyReader(Type type)
        {
            _instanceType = type;
            _reflectorCache = new ConcurrentDictionary<string, PropertyReflector>();
        }

        public object GetValue(object instance, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            var reflector = _reflectorCache.GetOrAdd(propertyName, name => _instanceType.GetTypeInfo().GetProperty(name)?.GetReflector());
            return reflector?.GetValue(instance);
        }
    }
}